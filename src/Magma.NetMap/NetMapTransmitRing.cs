using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using Magma.NetMap.Interop;

namespace Magma.NetMap
{
    public sealed unsafe class NetMapTransmitRing : NetMapRing
    {
        private const int SPINCOUNT = 100;
        private const int MAXLOOPTRY = 5;

        private object _sendBufferLock = new object();

        internal NetMapTransmitRing(byte* memoryRegion, ulong rxQueueOffset, int fileDescriptor)
            : base(memoryRegion, rxQueueOffset, fileDescriptor)
        {
        }

        public bool TryGetNextBuffer(out Memory<byte> buffer)
        {
            ref var ring = ref RingInfo[0];
            for (var loop = 0; loop < MAXLOOPTRY; loop++)
            {
                var slotIndex = GetCursor();
                if (slotIndex == -1)
                {
                    Thread.SpinWait(SPINCOUNT);
                    continue;
                }
                var slot = _rxRing[slotIndex];
                buffer = _bufferPool.GetBuffer(slot.buf_idx).Memory;
                return true;
            }
            buffer = default;
            return false;
        }

        public void SendBuffer(ReadOnlyMemory<byte> buffer)
        {
            if (!MemoryMarshal.TryGetMemoryManager(buffer, out NetMapOwnedMemory manager, out var start, out var length)
                || start != 0)
            {
                ExceptionHelper.ThrowInvalidOperation("Invalid buffer used for sendbuffer");
            }

            lock (_sendBufferLock)
            {
                var newHead = RingNext(RingInfo[0].head);
                ref var slot = ref _rxRing[newHead];
                if (slot.buf_idx != manager.BufferIndex)
                {
                    slot.buf_idx = manager.BufferIndex;
                    slot.flags = (ushort)(slot.flags | (ushort)netmap_slot_flags.NS_BUF_CHANGED);
                }
                slot.len = (ushort)buffer.Length;
                RingInfo[0].head = newHead;
            }
        }

        internal bool TrySendWithSwap(ref Netmap_slot sourceSlot)
        {
            ref var ring = ref RingInfo[0];
            for (var loop = 0; loop < MAXLOOPTRY; loop++)
            {
                lock (_sendBufferLock)
                {
                    var slotIndex = GetCursor();
                    if (slotIndex == -1)
                    {
                        Thread.SpinWait(SPINCOUNT);
                        continue;
                    }
                    
                    ref var slot = ref _rxRing[slotIndex];
                    var buffIndex = slot.buf_idx;
                    var buffSize = slot.len;
                    slot.buf_idx = sourceSlot.buf_idx;
                    slot.len = sourceSlot.len;
                    slot.flags = (ushort)(slot.flags | (uint)netmap_slot_flags.NS_BUF_CHANGED);

                    sourceSlot.buf_idx = buffIndex;
                    sourceSlot.len = buffSize;
                    sourceSlot.flags = (ushort)(sourceSlot.flags | (uint)netmap_slot_flags.NS_BUF_CHANGED);

                    ring.head = RingNext(slotIndex);
                    return true;
                }
            }
            Console.WriteLine("Dropped packet on swap");
            return false;
        }

        public void ForceFlush()
        {
            Unix.IOCtl(_fileDescriptor, Consts.NIOCTXSYNC, null);
            Console.WriteLine("Force Flush Called");
        }
}
}

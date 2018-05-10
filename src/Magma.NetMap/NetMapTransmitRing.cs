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
        private const int MAXLOOPTRY = 2;

        private object _sendBufferLock = new object();

        internal unsafe NetMapTransmitRing(string interfaceName, bool ishost, byte* memoryRegion, ulong rxQueueOffset, int fileDescriptor)
            : base(interfaceName, isTxRing: true, ishost, memoryRegion, rxQueueOffset)
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
                    ForceFlush();   // Thread.SpinWait(SPINCOUNT);
                    continue;
                }
                var slot = _rxRing[slotIndex];
                var manager = _bufferPool.GetBuffer(slot.buf_idx);
                manager.RingId = _ringId;
                buffer = manager.Memory;
                return true;
            }
            buffer = default;
            return false;
        }

        public void SendBuffer(ReadOnlyMemory<byte> buffer)
        {
            if (!MemoryMarshal.TryGetMemoryManager(buffer, out NetMapOwnedMemory manager, out var start, out var length))
            {
                ExceptionHelper.ThrowInvalidOperation("Invalid buffer used for sendbuffer");
            }
            if (start != 0)
            {
                ExceptionHelper.ThrowInvalidOperation("Invalid start for buffer");
            }
            if (manager.RingId != _ringId) ExceptionHelper.ThrowInvalidOperation($"Invalid ring id, expected {_ringId} actual {manager.RingId}");

            lock (_sendBufferLock)
            {
                var newHead = RingNext(RingInfo[0].head);
                ref var slot = ref _rxRing[RingInfo[0].head];
                if (slot.buf_idx != manager.BufferIndex)
                {
                    Console.WriteLine("Buffer Index Changed!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
                    slot.buf_idx = manager.BufferIndex;
                    slot.flags = (ushort)(slot.flags | (ushort)netmap_slot_flags.NS_BUF_CHANGED);
                }
                slot.len = (ushort)buffer.Length;
                RingInfo[0].head = newHead;
            }
        }

        internal bool TrySendWithSwap(ref Netmap_slot sourceSlot, ref Netmap_ring sourceRing)
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
                    sourceRing.cur = RingNext(sourceRing.cur);
                    ref var slot = ref _rxRing[slotIndex];
                    var buffIndex = slot.buf_idx;
                    slot.buf_idx = sourceSlot.buf_idx;
                    slot.len = sourceSlot.len;
                    slot.flags = (ushort)(slot.flags | (uint)netmap_slot_flags.NS_BUF_CHANGED);

                    sourceSlot.buf_idx = buffIndex;
                    sourceSlot.flags = (ushort)(sourceSlot.flags | (uint)netmap_slot_flags.NS_BUF_CHANGED);
                    sourceRing.head = sourceRing.cur;
                    ring.head = RingNext(slotIndex);
                    return true;
                }
            }
            Console.WriteLine("Dropped packet on swap");
            return false;
        }

        public void ForceFlush() => Unix.IOCtl(_fileDescriptor, Consts.NIOCTXSYNC, null);
    }
}

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
        private const int SPINCOUNT = 1000;
        private const int MAXLOOPTRY = 5;

        private object _getBufferLock = new object();
        private object _sendBufferLock = new object();

        internal NetMapTransmitRing(byte* memoryRegion, ulong rxQueueOffset, int fileDescriptor)
            : base(memoryRegion, rxQueueOffset, fileDescriptor)
        {
        }
               
        public bool TryGetNextBuffer(out Memory<byte> buffer)
        {
            lock(_getBufferLock)
            {
                ref var ring = ref RingInfo[0];
                for (var loop = 0; loop < MAXLOOPTRY; loop++)
                {
                    if (IsRingEmpty())
                    {
                        Thread.SpinWait(SPINCOUNT);
                        continue;
                    }
                    var i = ring.cur;
                    var slot = _rxRing[i];
                    ring.cur = RingNext(ring.cur);
                    buffer = _bufferPool.GetBuffer(slot.buf_idx).Memory;
                    return true;
                }
                buffer = default;
                return false;
            }
        }

        public void SendBuffer(ReadOnlyMemory<byte> buffer)
        {
            if(!MemoryMarshal.TryGetMemoryManager(buffer, out NetMapOwnedMemory manager, out var start, out var length))
            {
                throw new InvalidOperationException("Not one of our buffers whatcha up to fool?");
            }
            if (start != 0) throw new InvalidOperationException("Data not started at the start clown");

            lock(_sendBufferLock)
            {
                var newHead = RingNext(RingInfo[0].head);
                ref var slot = ref _rxRing[newHead];
                slot.flags = (ushort)(slot.flags | (ushort)netmap_slot_flags.NS_BUF_CHANGED);
                slot.len = (ushort)buffer.Length;
                slot.buf_idx = manager.BufferIndex;
                RingInfo[0].head = newHead;
            }
        }

        internal bool TrySendWithSwap(ref Netmap_slot sourceSlot)
        {
            ref var ring = ref RingInfo[0];
            for(var loop = 0; loop < MAXLOOPTRY; loop++)
            {
                if (IsRingEmpty())
                {
                    //Need to poll
                    Thread.SpinWait(SPINCOUNT);
                    continue;
                }

                Console.WriteLine($"Swapping data on ring {_ringId}");
                var i = ring.cur;
                var iNext = RingNext(i);
                ring.cur = iNext;

                ref var slot = ref _rxRing[i];
                var buffIndex = slot.buf_idx;
                var buffSize = slot.len;
                slot.buf_idx = sourceSlot.buf_idx;
                slot.len = sourceSlot.len;
                slot.flags = (ushort)(_rxRing[i].flags | (uint)netmap_slot_flags.NS_BUF_CHANGED);

                sourceSlot.buf_idx = buffIndex;
                sourceSlot.len = buffSize;
                sourceSlot.flags = (ushort)(sourceSlot.flags | (uint)netmap_slot_flags.NS_BUF_CHANGED);

                ring.head = RingNext(i);
                return true;
            }
            return false;
        }
    }
}

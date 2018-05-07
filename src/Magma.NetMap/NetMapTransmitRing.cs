using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Magma.NetMap.Interop;

namespace Magma.NetMap
{
    public sealed unsafe class NetMapTransmitRing : NetMapRing
    {
        private const int SPINCOUNT = 1000;
        private const int MAXLOOPTRY = 5;

        internal NetMapTransmitRing(byte* memoryRegion, ulong rxQueueOffset, int fileDescriptor)
            : base(memoryRegion, rxQueueOffset, fileDescriptor)
        {
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

        public bool TrySend(Span<byte> buffer)
        {
            ref var ring = ref RingInfo[0];
            
            for(var loop = 0; loop < MAXLOOPTRY; loop++)
            { 
                if(IsRingEmpty())
                {
                    //Need to poll
                    Thread.SpinWait(SPINCOUNT);
                    continue;
                }
                Console.WriteLine($"Sending data on ring {_ringId} size is {buffer.Length}");
                var i = ring.cur;
                var iNext = RingNext(i);
                ref var slot = ref _rxRing[i];
                ring.cur = iNext;

                var outBuffer = GetBuffer(slot.buf_idx);
                buffer.CopyTo(outBuffer);
                slot.len = (ushort)buffer.Length;
                ring.head = iNext;
                return true;
            }
            return false;
        }

        public bool TryGetNextBuffer(out Span<byte> buffer)
        {
            ref var ring = ref RingInfo[0];
            for(var loop = 0; loop < MAXLOOPTRY; loop++)
            {
                if(IsRingEmpty())
                {
                    Thread.SpinWait(SPINCOUNT);
                    continue;
                }
                var i = ring.cur;
                var slot = _rxRing[i];
                ring.cur = RingNext(ring.cur);
                buffer = GetBuffer(slot.buf_idx);
                return true;
            }
            buffer = default;
            return false;
        }

        public bool TrySendMore(int previousSize, out Span<byte> buffer)
        {
            ref var ring = ref RingInfo[0];
            var i = ring.cur;
            _rxRing[i - 1].len = (ushort)previousSize;
            for(var loop = 0; loop < MAXLOOPTRY;loop++)
            {
                if (IsRingEmpty())
                {
                    Thread.SpinWait(SPINCOUNT);
                    continue;
                }
                
                var slot = _rxRing[i];
                ring.cur = RingNext(ring.cur);
                buffer = GetBuffer(slot.buf_idx);
                return true;
            }
            buffer = default;
            return false;
        }

        public void SendBuffer(int size)
        {
            var i = RingInfo[0].cur - 1;
            _rxRing[i].len = (ushort)size;
            RingInfo[0].head = RingInfo[0].cur;
        }
    }
}

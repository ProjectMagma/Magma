using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Magma.NetMap.Interop;

namespace Magma.NetMap
{
    public sealed unsafe class NetMapTransmitRing : NetMapRing
    {

        internal NetMapTransmitRing(byte* memoryRegion, ulong rxQueueOffset, int fileDescriptor)
            : base(memoryRegion, rxQueueOffset, fileDescriptor)
        {
        }

        public void Send(Span<byte> buffer)
        {
            while(true)
            {
                if(IsRingEmpty())
                {
                    //Need to poll
                    Thread.SpinWait(1000);
                    continue;
                }
                Console.WriteLine($"Sending data on ring {_ringId} size is {buffer.Length}");
                var i = RxRingInfo[0].cur;
                var slot = _rxRing[i];
                var outBuffer = GetBuffer(slot.buf_idx).Slice(0, slot.len);
                buffer.CopyTo(outBuffer);
                _rxRing[i].len = (ushort)buffer.Length;
                RxRingInfo[0].head = RxRingInfo[0].cur = RingNext(i);
                return;
            }
        }

        public Span<byte> GetNextBuffer()
        {
            while(true)
            {
                if(IsRingEmpty())
                {
                    Thread.SpinWait(1000);
                }
                var i = RxRingInfo[0].cur;
                var slot = _rxRing[i];
                return GetBuffer(slot.buf_idx);
            }
        }

        public void SendBuffer(int size)
        {
            var i = RxRingInfo[0].cur;
            _rxRing[i].len = (ushort)size;
            RxRingInfo[0].head = RxRingInfo[0].cur = RingNext(i);
        }
    }
}

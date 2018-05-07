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
                    Thread.Sleep(10);
                    continue;
                }

                var i = RxRingInfo[0].cur;
                var slot = _rxRing[i];
                var outBuffer = GetBuffer(slot.buf_idx).Slice(0, slot.len);
                buffer.CopyTo(outBuffer);
                _rxRing[i].len = (ushort)buffer.Length;
                RxRingInfo[0].head = RxRingInfo[0].cur = RingNext(i);
            }
        }
    }
}

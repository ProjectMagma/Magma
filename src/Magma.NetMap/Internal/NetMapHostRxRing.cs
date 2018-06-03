using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using Magma.NetMap.Interop;
using Magma.Network.Header;
using static Magma.NetMap.Interop.Libc;

namespace Magma.NetMap.Internal
{
    internal sealed class NetMapHostRxRing:NetMapRing
    {
        private readonly Thread _worker;
        private readonly NetMapTransmitRing _transmitRing;

        internal unsafe NetMapHostRxRing(RxTxPair rxTxPair, byte* memoryRegion, long rxQueueOffset, NetMapTransmitRing transmitRing)
            : base(rxTxPair, memoryRegion, rxQueueOffset)
        {
            _transmitRing = transmitRing;
            _worker = new Thread(new ThreadStart(ThreadLoop));
        }

        public override void Start() => _worker.Start();

        internal override void Return(int buffer_index) => throw new NotImplementedException();

        private void ThreadLoop()
        {
            ref var ring = ref RingInfo;
            while (true)
            {
                while (!IsRingEmpty())
                {
                    //Console.WriteLine("Received data on host ring");
                    var i = ring.Cursor;
                    ring.Cursor = RingNext(i);
                    if(_transmitRing.TryGetNextBuffer(out var copyBuffer))
                    {
                        ref var slot = ref GetSlot(i);
                        var bufferSource = GetBuffer(slot.buf_idx);
                        bufferSource.CopyTo(copyBuffer.Span);
                        _transmitRing.SendBuffer(copyBuffer.Slice(0, slot.len));
                    }
                    ring.Head = RingNext(ring.Head);
                }
                _rxTxPair.WaitForWork();
            }
        }
    }
}

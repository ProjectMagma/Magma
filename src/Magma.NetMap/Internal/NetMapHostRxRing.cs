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
                var sentData = false;
                while (!IsRingEmpty())
                {
                    //Console.WriteLine("Received data on host ring");
                    var i = ring.Cursor;
                    ring.Cursor = RingNext(i);
                    _transmitRing.TrySendWithSwap(ref GetSlot(i));
                    ring.Head = RingNext(ring.Head);
                    sentData = true;
                }
                if(sentData) _transmitRing.ForceFlush();
                _rxTxPair.WaitForWork();
            }
        }
    }
}

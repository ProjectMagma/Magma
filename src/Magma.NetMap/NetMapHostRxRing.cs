using System.Threading;
using Magma.NetMap.Interop;
using static Magma.NetMap.Interop.Libc;

namespace Magma.NetMap
{
    public sealed class NetMapHostRxRing:NetMapRing
    {
        private readonly Thread _worker;
        private readonly NetMapTransmitRing _transmitRing;
        
        internal unsafe NetMapHostRxRing(RxTxPair rxTxPair, byte* memoryRegion, ulong rxQueueOffset, NetMapTransmitRing transmitRing)
            : base(rxTxPair, memoryRegion, rxQueueOffset)
        {
            _transmitRing = transmitRing;
            _worker = new Thread(new ThreadStart(ThreadLoop))
            {
                IsBackground = true
            };
            _worker.Start();
        }

        private void ThreadLoop()
        {
            ref var ring = ref RingInfo();
            while (true)
            {
                _rxTxPair.WaitForWork();

                var sentData = false;
                while (!IsRingEmpty())
                {
                    var i = ring.Cursor;
                    
                    _transmitRing.TrySendWithSwap(ref GetSlot(i), ref ring);
                    sentData = true;
                }
                if (sentData) _transmitRing.ForceFlush();
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using Magma.NetMap.Interop;
using Magma.Network.Header;
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
            _worker = new Thread(new ThreadStart(ThreadLoop));
            _worker.Start();
        }

        private void ThreadLoop()
        {
            ref var ring = ref RingInfo();
            while (true)
            {
                var sentData = false;
                while (!IsRingEmpty())
                {
                    //Console.WriteLine("Received data on host ring");
                    var i = ring.Cursor;
                    
                    _transmitRing.TrySendWithSwap(ref GetSlot(i), ref ring);
                    //RingInfo[0].flags = (ushort)(RingInfo[0].flags | (ushort)netmap_slot_flags.NS_BUF_CHANGED);
                    sentData = true;
                }
                if(sentData) _transmitRing.ForceFlush();
                _rxTxPair.WaitForWork();
            }
        }
    }
}

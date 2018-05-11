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

        internal unsafe NetMapHostRxRing(string interfaceName, byte* memoryRegion, ulong rxQueueOffset, FileDescriptor fileDescriptor, NetMapTransmitRing transmitRing)
            : base(interfaceName, isTxRing : false, isHost:true, memoryRegion, rxQueueOffset)
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
                var fd = new PollFileDescriptor()
                {
                    Events = PollEvents.POLLIN,
                    Fd = _fileDescriptor
                };

                var pollResult = Libc.Poll(ref fd, 1, -1);
                if (pollResult < 0)
                {
                    //Console.WriteLine($"Poll failed on ring {_ringId} exiting polling loop");
                    return;
                }
                var sentData = false;
                while (!IsRingEmpty())
                {
                    //Console.WriteLine("Received data on host ring");
                    var i = ring.cur;
                    
                    _transmitRing.TrySendWithSwap(ref GetSlot(i), ref ring);
                    //RingInfo[0].flags = (ushort)(RingInfo[0].flags | (ushort)netmap_slot_flags.NS_BUF_CHANGED);
                    sentData = true;
                }
                if(sentData) _transmitRing.ForceFlush();
            }
        }
    }
}

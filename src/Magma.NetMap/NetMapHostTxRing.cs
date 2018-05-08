using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using Magma.NetMap.Interop;
using Magma.Network.Header;

namespace Magma.NetMap
{
    public sealed unsafe class NetMapHostTxRing:NetMapRing
    {
        private readonly Thread _worker;
        private readonly NetMapTransmitRing _transmitRing;

        internal NetMapHostTxRing(byte* memoryRegion, ulong rxQueueOffset, int fileDescriptor, NetMapTransmitRing transmitRing)
            : base(memoryRegion, rxQueueOffset, fileDescriptor)
        {
            _transmitRing = transmitRing;
            _worker = new Thread(new ThreadStart(ThreadLoop));
            _worker.Start();
        }

        private void ThreadLoop()
        {
            ref var ring = ref RingInfo[0];
            while (true)
            {
                var fd = new Unix.pollFd()
                {
                    Events = Unix.PollEvents.POLLIN,
                    Fd = _fileDescriptor
                };

                var pollResult = Unix.poll(ref fd, 1, 100);
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
                    var iNext = RingNext(i);
                    ring.cur = iNext;
                    _transmitRing.TrySendWithSwap(ref _rxRing[i]);
                    ring.head = iNext;
                    sentData = true;
                }
                if(sentData) _transmitRing.ForceFlush();
            }
        }
    }
}

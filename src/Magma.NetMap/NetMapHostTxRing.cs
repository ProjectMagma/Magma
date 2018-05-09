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
            : base(memoryRegion, rxQueueOffset)
        {
            _fileDescriptor = fileDescriptor;
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

                var pollResult = Unix.poll(ref fd, 1, Consts.POLLTIME);
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
                    
                    _transmitRing.TrySendWithSwap(ref _rxRing[i], ref ring);
                    RingInfo[0].flags = (ushort)(RingInfo[0].flags | (ushort)netmap_slot_flags.NS_BUF_CHANGED);
                    
                    sentData = true;
                }
                if(sentData) _transmitRing.ForceFlush();
            }
        }
    }
}

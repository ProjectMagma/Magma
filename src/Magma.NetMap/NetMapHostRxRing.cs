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
    public sealed unsafe class NetMapHostRxRing:NetMapRing
    {
        private readonly Thread _worker;
        private readonly NetMapTransmitRing _transmitRing;

        internal NetMapHostRxRing(string interfaceName, byte* memoryRegion, ulong rxQueueOffset, int fileDescriptor, NetMapTransmitRing transmitRing)
            : base(memoryRegion, rxQueueOffset)
        {
            _fileDescriptor = Unix.Open("/dev/netmap", Unix.OpenFlags.O_RDWR);
            if (_fileDescriptor < 0) throw new InvalidOperationException($"Need to handle properly (release memory etc) error was {_fileDescriptor}");
            var request = new NetMapRequest
            {
                nr_cmd = 0,
                nr_flags = (uint)nr_flags.NR_RX_RINGS_ONLY,
                nr_ringid = (ushort)_ringId,
                nr_version = Consts.NETMAP_API,
            };
            Console.WriteLine($"Getting FD for Receive RingID {_ringId}");
            var textbytes = Encoding.ASCII.GetBytes(interfaceName + "\0");
            fixed (void* txtPtr = textbytes)
            {
                Buffer.MemoryCopy(txtPtr, request.nr_name, textbytes.Length, textbytes.Length);
            }


            if (Unix.IOCtl(_fileDescriptor, Consts.NIOCREGIF, &request) != 0)
            {
                throw new InvalidOperationException("Failed to open an FD for a single ring");
            }
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
                    //RingInfo[0].flags = (ushort)(RingInfo[0].flags | (ushort)netmap_slot_flags.NS_BUF_CHANGED);
                    
                    sentData = true;
                }
                if(sentData) _transmitRing.ForceFlush();
            }
        }
    }
}

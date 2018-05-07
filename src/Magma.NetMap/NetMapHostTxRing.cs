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
                    Console.WriteLine($"Poll failed on ring {_ringId} exiting polling loop");
                    return;
                }

                while (!IsRingEmpty())
                {
                    Console.WriteLine("Received data on host ring");
                    var i = RxRingInfo[0].cur;
                    var slot = _rxRing[i];
                    var buffer = GetBuffer(slot.buf_idx).Slice(0, slot.len);
                    TryConsume(ref buffer, out var eth);

                    _transmitRing.SendWithSwap(_rxRing,(int) i);
                    Console.WriteLine("Passed on host data to a tx ring");
                    RxRingInfo[0].head = RxRingInfo[0].cur = RingNext(i);
                }
            }
        }

        public static bool TryConsume(ref Span<byte> span, out Ethernet ethernet)
        {
            const int CrcSize = 4;

            if (span.Length >= Unsafe.SizeOf<Ethernet>() + CrcSize)
            {
                ethernet = Unsafe.As<byte, Ethernet>(ref MemoryMarshal.GetReference(span));
                // CRC check
                span = span.Slice(Unsafe.SizeOf<Ethernet>(), span.Length - (Unsafe.SizeOf<Ethernet>() + CrcSize));
                Console.WriteLine("There was a crc");
                return true;
            }
            else
            {
                Console.WriteLine("There was no crc");
            }

            ethernet = default;
            return false;
        }
    }
}

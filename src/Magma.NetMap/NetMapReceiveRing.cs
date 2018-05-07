using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using Magma.NetMap.Interop;

namespace Magma.NetMap
{
    public sealed unsafe class NetMapReceiveRing : NetMapRing
    {
        private readonly Thread _worker;

        internal NetMapReceiveRing(byte* memoryRegion, ulong rxQueueOffset, int fileDescriptor)
            : base(memoryRegion, rxQueueOffset, fileDescriptor)
        {
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
                    var i = RxRingInfo[0].cur;
                    var slot = _rxRing[i];
                    var buffer = GetBuffer(slot.buf_idx).Slice(0, slot.len);
                    if (!TryConsume(buffer))
                    {
                        RxRingInfo[0].flags = RxRingInfo[0].flags | (uint)netmap_slot_flags.NS_FORWARD;
                        _rxRing[i].flags = (ushort)(_rxRing[i].flags | (ushort)netmap_slot_flags.NS_FORWARD);
                        Console.WriteLine("Forwarded to host");
                    }
                    RxRingInfo[0].head = RxRingInfo[0].cur = RingNext(i);
                }
            }
        }

        private bool TryConsume(Span<byte> buffer)
        {
            Console.WriteLine($"Received packet on ring {_ringId} size was {buffer.Length}");
            return false;
        }

        private void PrintSlotInfo(int index)
        {
            var slot = _rxRing[index];
            Console.WriteLine($"Slot {index} bufferIndex {slot.buf_idx} flags {slot.flags} length {slot.len} pointer {slot.ptr}");
        }
    }
}

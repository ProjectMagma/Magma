using System;
using System.Threading;
using Magma.NetMap.Interop;
using Magma.Network.Abstractions;

namespace Magma.NetMap
{
    public sealed unsafe class NetMapReceiveRing<TPacketReceiver> : NetMapRing
        where TPacketReceiver : IPacketReceiver
    {
        private readonly Thread _worker;
        private TPacketReceiver _receiver;

        internal NetMapReceiveRing(byte* memoryRegion, ulong rxQueueOffset, int fileDescriptor, TPacketReceiver receiver)
            : base(memoryRegion, rxQueueOffset, fileDescriptor)
        {
            _receiver = receiver;
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
                    var nexti = RingNext(i);
                    var slot = _rxRing[i];
                    var buffer = GetBuffer(slot.buf_idx).Slice(0, slot.len);
                    RxRingInfo[0].cur = nexti;
                    if (!_receiver.TryConsume(_ringId, buffer))
                    {
                        RxRingInfo[0].flags = RxRingInfo[0].flags | (uint)netmap_slot_flags.NS_FORWARD;
                        _rxRing[i].flags = (ushort)(_rxRing[i].flags | (ushort)netmap_slot_flags.NS_FORWARD);
                        Console.WriteLine("Forwarded to host");
                    }
                    RxRingInfo[0].head = nexti;
                }
            }
        }

        private void PrintSlotInfo(int index)
        {
            var slot = _rxRing[index];
            Console.WriteLine($"Slot {index} bufferIndex {slot.buf_idx} flags {slot.flags} length {slot.len} pointer {slot.ptr}");
        }
    }
}

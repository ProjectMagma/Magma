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

            Console.WriteLine("Slots Start at");
            PrintSlotInfo(0);
            Console.WriteLine("Slots End at");
            PrintSlotInfo(_numberOfSlots - 1);
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

                while (!IsRingEmpty())
                {

                    var i = ring.cur;
                    var nexti = RingNext(i);
                    ring.cur = nexti;
                    ref var slot = ref _rxRing[i];
                    var buffer = GetBuffer(slot.buf_idx, slot.len);
                    if (!_receiver.TryConsume(_ringId, buffer))
                    {
                        //ring.flags = ring.flags | (uint)netmap_slot_flags.NS_FORWARD;
                        slot.flags = (ushort)(slot.flags | (ushort)netmap_slot_flags.NS_FORWARD);
                        //Console.WriteLine("Forwarded to host");
                    }
                    ring.head = nexti;
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

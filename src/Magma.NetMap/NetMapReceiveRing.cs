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
        private NetMapTransmitRing _hostTxRing;

        internal NetMapReceiveRing(byte* memoryRegion, ulong rxQueueOffset, int fileDescriptor, TPacketReceiver receiver, NetMapTransmitRing hostTxRing)
            : base(memoryRegion, rxQueueOffset)
        {
            _fileDescriptor = fileDescriptor;
            _hostTxRing = hostTxRing;
            _receiver = receiver;
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

                while (!IsRingEmpty())
                {

                    var i = ring.cur;
                    var nexti = RingNext(i);
                    ring.cur = nexti;
                    ref var slot = ref _rxRing[i];
                    var buffer = GetBuffer(slot.buf_idx, slot.len);
                    if (!_receiver.TryConsume(_ringId, buffer))
                    {
                        _hostTxRing.TrySendWithSwap(ref slot, ref ring);
                        _hostTxRing.ForceFlush();
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

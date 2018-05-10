using System;
using System.Text;
using System.Threading;
using Magma.NetMap.Interop;
using Magma.Network.Abstractions;

namespace Magma.NetMap
{
    public sealed class NetMapReceiveRing<TPacketReceiver> : NetMapRing
        where TPacketReceiver : IPacketReceiver
    {
        private readonly Thread _worker;
        private TPacketReceiver _receiver;
        private NetMapTransmitRing _hostTxRing;

        internal unsafe NetMapReceiveRing(string interfaceName, byte* memoryRegion, ulong rxQueueOffset, int fileDescriptor, TPacketReceiver receiver, NetMapTransmitRing hostTxRing)
            : base(interfaceName, isTxRing:false, isHost:false, memoryRegion, rxQueueOffset)
        {
            _hostTxRing = hostTxRing;
            _receiver = receiver;
            _worker = new Thread(new ThreadStart(ThreadLoop));
            _worker.Start();
        }

        private void ThreadLoop()
        {
            ref var ring = ref RingInfo();
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
                    Console.WriteLine($"Poll failed on ring {_ringId} exiting polling loop");
                    return;
                }

                while (!IsRingEmpty())
                {

                    var i = ring.cur;
                    var nexti = RingNext(i);
                    ref var slot = ref GetSlot(i);
                    var buffer = GetBuffer(slot.buf_idx, slot.len);
                    if (!_receiver.TryConsume(_ringId, buffer))
                    {
                        _hostTxRing.TrySendWithSwap(ref slot, ref ring);
                        _hostTxRing.ForceFlush();
                        //Console.WriteLine("Forwarded to host");
                    }
                    else
                    {
                        ring.cur = nexti;
                        ring.head = nexti;
                    }
                    
                }
            }
        }

        private void PrintSlotInfo(int index)
        {
            var slot = GetSlot(index);
            Console.WriteLine($"Slot {index} bufferIndex {slot.buf_idx} flags {slot.flags} length {slot.len} pointer {slot.ptr}");
        }
    }
}

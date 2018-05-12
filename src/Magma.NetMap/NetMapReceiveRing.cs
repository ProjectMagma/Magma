using System;
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

        internal unsafe NetMapReceiveRing(RxTxPair rxTxPair, byte* memoryRegion, long queueOffset, TPacketReceiver receiver, NetMapTransmitRing hostTxRing)
            : base(rxTxPair, memoryRegion, queueOffset)
        {
            _hostTxRing = hostTxRing;
            _receiver = receiver;
            _worker = new Thread(new ThreadStart(ThreadLoop));
            _worker.IsBackground = true;
        }

        public void Start() => _worker.Start();

        private void ThreadLoop()
        {
            ref var ring = ref RingInfo();
            Console.WriteLine("Receive Ring Started");
            while (true)
            {
                while (!IsRingEmpty())
                {

                    var i = ring.Cursor;
                    ref var slot = ref GetSlot(i);
                    var buffer = _bufferPool.GetBuffer(slot.buf_idx);
                    buffer.RingId = _ringId;
                    buffer.Length = slot.len;
                    Console.WriteLine($"Received data on ring {_ringId} slot id {i} length {slot.len}");
                    ring.Cursor = i;
                    if (!_receiver.TryConsume(buffer))
                    {
                        _hostTxRing.TrySendWithSwap(ref slot);
                        _hostTxRing.ForceFlush();
                        Console.WriteLine("Received buffer and passed it on");
                    }
                    ring.Head = RingNext(ring.Head);
                }
                _rxTxPair.WaitForWork();
            }
        }

        private void PrintSlotInfo(int index)
        {
            var slot = GetSlot(index);
            Console.WriteLine($"Slot {index} bufferIndex {slot.buf_idx} flags {slot.flags} length {slot.len} pointer {slot.ptr}");
        }
    }
}

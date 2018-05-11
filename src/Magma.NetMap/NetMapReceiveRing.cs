using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using Magma.NetMap.Interop;
using Magma.Network.Abstractions;
using static Magma.NetMap.Interop.Libc;

namespace Magma.NetMap
{
    public sealed class NetMapReceiveRing<TPacketReceiver> : NetMapRing
        where TPacketReceiver : IPacketReceiver
    {
        private readonly Thread _worker;
        private TPacketReceiver _receiver;
        private NetMapTransmitRing _hostTxRing;

        internal unsafe NetMapReceiveRing(byte* memoryRegion, RxTxPair rxTxPair, ulong rxQueueOffset, TPacketReceiver receiver, NetMapTransmitRing hostTxRing)
            : base(rxTxPair, memoryRegion, rxQueueOffset)
        {
            _hostTxRing = hostTxRing;
            _receiver = receiver;
            _worker = new Thread(new ThreadStart(ThreadLoop));
            _worker.IsBackground = true;
            _worker.Start();
        }

        private void ThreadLoop()
        {
            ref var ring = ref RingInfo();
            while (true)
            {
                while (!IsRingEmpty())
                {
                    var i = ring.Cursor;
                    var nexti = RingNext(i);
                    ref var slot = ref GetSlot(i);
                    var buffer = GetBuffer(slot.buf_idx, slot.len);
                    if (!_receiver.TryConsume(_ringId, buffer))
                    {
                        _hostTxRing.TrySendWithSwap(ref slot, ref ring);
                        _hostTxRing.ForceFlush();
                    }
                    else
                    {
                        ring.Cursor = nexti;
                        ring.Head = nexti;
                    }
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

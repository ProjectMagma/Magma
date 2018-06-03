using System;
using System.Threading;
using Magma.NetMap.Interop;
using Magma.Network.Abstractions;

namespace Magma.NetMap.Internal
{
    internal sealed class NetMapReceiveRing<TPacketReceiver> : INetMapRing
        where TPacketReceiver : IPacketReceiver
    {
        private readonly Thread _worker;
        private TPacketReceiver _receiver;
        private NetMapTransmitRing _hostTxRing;
        private readonly object _lock = new object();
        private NetMapBufferPool _bufferPool;
        private readonly NetMapRing _netMapRing;

        internal unsafe NetMapReceiveRing(string interfaceName, byte* memoryRegion, long queueOffset, TPacketReceiver receiver, NetMapTransmitRing hostTxRing)
        {
            _hostTxRing = hostTxRing;
            _receiver = receiver;
            _netMapRing = new NetMapRing(interfaceName, isHostStack: false, isTxRing: false, memoryRegion, queueOffset);
            _worker = new Thread(new ThreadStart(ThreadLoop));
        }

        public NetMapBufferPool BufferPool { set => _bufferPool = value; }
        public NetMapRing NetMapRing => _netMapRing;

        public void Start() => _worker.Start();

        private void ThreadLoop()
        {
            ref var ring = ref _netMapRing.RingInfo;
            while (true)
            {
                while (!_netMapRing.IsRingEmpty())
                {
                    var i = ring.Cursor;
                    ref var slot = ref _netMapRing.GetSlot(i);
                    var buffer = _bufferPool.GetBuffer(slot.buf_idx);
                    buffer.RingId = this;
                    buffer.Length = slot.len;
                    ring.Cursor = _netMapRing.RingNext(i);
                    if (!_receiver.TryConsume(new NetMapMemoryWrapper(buffer)).IsEmpty)
                    {
                        if(_hostTxRing.TryGetNextBuffer(out var copyMemory))
                        {
                            buffer.Memory.CopyTo(copyMemory);
                            _hostTxRing.SendBuffer(copyMemory.Slice(0, slot.len));
                        }
                        MoveHeadForward(slot.buf_idx);
                    }
                }
                _receiver.FlushPendingAcks();
                //Add a little spin to check
                Thread.SpinWait(100);
                if (!_netMapRing.IsRingEmpty()) continue;

                _netMapRing.WaitForWork();
            }
        }

        private void MoveHeadForward(uint bufferIndex)
        {
            lock(_lock)
            {
                ref var ring = ref _netMapRing.RingInfo;
                ref var slot = ref _netMapRing.GetSlot(ring.Head);
                if(slot.buf_idx != bufferIndex)
                {
                    slot.buf_idx = bufferIndex;
                    slot.flags |= Netmap.NetmapSlotFlags.NS_BUF_CHANGED;
                }
                ring.Head = _netMapRing.RingNext(ring.Head);
            }
        }

        private void PrintSlotInfo(int index)
        {
            var slot = _netMapRing.GetSlot(index);
            Console.WriteLine($"Slot {index} bufferIndex {slot.buf_idx} flags {slot.flags} length {slot.len} pointer {slot.ptr}");
        }

        public void Return(int buffer_index) => MoveHeadForward((uint)buffer_index);

        public void Dispose() => _netMapRing.Dispose();
    }
}

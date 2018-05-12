using System;
using System.Runtime.InteropServices;
using System.Threading;
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
        private object _headLock = new object();

        internal unsafe NetMapReceiveRing(string interfaceName, byte* memoryRegion, ulong rxQueueOffset, FileDescriptor fileDescriptor, TPacketReceiver receiver, NetMapTransmitRing hostTxRing)
            : base(interfaceName, isTxRing: false, isHost: false, memoryRegion, rxQueueOffset)
        {
            _hostTxRing = hostTxRing;
            _receiver = receiver;
            _worker = new Thread(new ThreadStart(ThreadLoop));
            _worker.Start();
        }

        private void ThreadLoop()
        {
            ref var ring = ref RingInfo();
            var epoll = EPollCreate(0);
            if (epoll.Pointer < 0) ExceptionHelper.ThrowInvalidOperation("Failed to get Epoll handle");
            var epollEvent = new EPollEvent()
            {
                data = new EPollData() { FileDescriptor = _fileDescriptor, },
                events = EPollEvents.EPOLLIN,
            };
            if (EPollControl(epoll, EPollCommand.EPOLL_CTL_ADD, _fileDescriptor, ref epollEvent) != 0) ExceptionHelper.ThrowInvalidOperation("Epoll failed");

            Span<EPollEvent> events = stackalloc EPollEvent[4];

            while (true)
            {
                while (!IsRingEmpty())
                {

                    var i = ring.Cursor;
                    var nexti = RingNext(i);
                    ref var slot = ref GetSlot(i);
                    var buffer = _bufferPool.GetBuffer(slot.buf_idx);
                    if (!_receiver.TryConsume(new NetmapMemoryWrapper( buffer)).IsEmpty)
                    {
                        _hostTxRing.TrySendWithSwap(ref slot, ref ring);
                        _hostTxRing.ForceFlush();
                    }
                    else
                    {
                        ring.Cursor = nexti;
                    }
                }

                var numberOfEvents = EPollWait(epoll, ref MemoryMarshal.GetReference(events), 4, -1);
            }
        }

        private void PrintSlotInfo(int index)
        {
            var slot = GetSlot(index);
            Console.WriteLine($"Slot {index} bufferIndex {slot.buf_idx} flags {slot.flags} length {slot.len} pointer {slot.ptr}");
        }

        internal override void ReturnMemory(NetMapOwnedMemory ownedMemory)
        {
            lock(_headLock)
            {
                ref var ring = ref RingInfo();
                var head = GetSlot(ring.Head);
                head.buf_idx = ownedMemory.BufferIndex;
                ring.Head = RingNext(ring.Head);
            }
        }
    }
}

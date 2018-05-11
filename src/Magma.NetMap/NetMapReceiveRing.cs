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

        internal unsafe NetMapReceiveRing(string interfaceName, byte* memoryRegion, ulong rxQueueOffset, TPacketReceiver receiver, NetMapTransmitRing hostTxRing)
            : base(interfaceName, isTxRing: false, isHost: false, memoryRegion, rxQueueOffset)
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
            var epoll = Libc.EPollCreate(0);
            if (epoll.Pointer < 0) ExceptionHelper.ThrowInvalidOperation("Failed to get Epoll handle");
            var epollEvent = new Libc.EPollEvent()
            {
                data = new Libc.EPollData() { FileDescriptor = _fileDescriptor, },
                events = Libc.EPollEvents.EPOLLIN ,
            };
            if (Libc.EPollControl(epoll, Libc.EPollCommand.EPOLL_CTL_ADD, _fileDescriptor, ref epollEvent) != 0) ExceptionHelper.ThrowInvalidOperation("Epoll failed");

            Span<Libc.EPollEvent> events = stackalloc Libc.EPollEvent[4];

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

                var numberOfEvents = Libc.EPollWait(epoll, ref MemoryMarshal.GetReference(events), 4, -1);
            }
        }

        private void PrintSlotInfo(int index)
        {
            var slot = GetSlot(index);
            Console.WriteLine($"Slot {index} bufferIndex {slot.buf_idx} flags {slot.flags} length {slot.len} pointer {slot.ptr}");
        }
    }
}

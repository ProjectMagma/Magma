using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using Magma.NetMap.Interop;
using static Magma.NetMap.Interop.Netmap;

namespace Magma.NetMap.Internal
{
    internal sealed class NetMapTransmitRing : NetMapAbstractRing
    {
        private const int SPINCOUNT = 100;
        private const int MAXLOOPTRY = 2;
        private ManualResetEventSlim _sendEvent = new ManualResetEventSlim(true);
        private Queue<(NetMapOwnedMemory ownedMemory, ushort length)> _sendQueue = new Queue<(NetMapOwnedMemory ownedMemory, ushort length)>(1024);
        private SpinLock _lock = new SpinLock(enableThreadOwnerTracking: false);
        private Thread _flushThread;

        internal unsafe NetMapTransmitRing(RxTxPair rxTxPair, byte* memoryRegion, long queueOffset)
            : base(rxTxPair, memoryRegion, queueOffset) => _flushThread = new Thread(FlushLoop);

        public override void Start() => _flushThread.Start();

        private void FlushLoop()
        {
            ref var ring = ref RingInfo;
            while (true)
            {
                var dataWritten = false;
                var hasLock = false;
                try
                {
                    _lock.Enter(ref hasLock);
                    while (_sendQueue.TryDequeue(out var queuedItem))
                    {
                        var newHead = RingNext(ring.Head);
                        ref var slot = ref GetSlot(ring.Head);
                        if (slot.buf_idx != queuedItem.ownedMemory.BufferIndex)
                        {
                            slot.buf_idx = queuedItem.ownedMemory.BufferIndex;
                            slot.flags |= NetmapSlotFlags.NS_BUF_CHANGED;
                        }
                        slot.len = queuedItem.length;
                        ring.Head = newHead;
                        dataWritten = true;
                    }
                }
                finally
                {
                    if (hasLock) _lock.Exit(true);
                }
                if(dataWritten) _rxTxPair.ForceFlush();
                if (_sendQueue.Count > 0) continue;
                _sendEvent.Wait();
                _sendEvent.Reset();

            }
        }

        public bool TryGetNextBuffer(out Memory<byte> buffer)
        {
            ref var ring = ref RingInfo;
            for (var loop = 0; loop < MAXLOOPTRY; loop++)
            {
                var slotIndex = GetCursor();
                if (slotIndex == -1)
                {
                    Thread.SpinWait(SPINCOUNT);
                    continue;
                }
                var slot = GetSlot(slotIndex);
                var manager = _bufferPool.GetBuffer(slot.buf_idx);
                manager.RingId = this;
                manager.Length = (ushort)_bufferSize;
                buffer = manager.Memory;
                return true;
            }
            buffer = default;
            return false;
        }

        public void SendBuffer(ReadOnlyMemory<byte> buffer)
        {
            if (!MemoryMarshal.TryGetMemoryManager(buffer, out NetMapOwnedMemory manager, out var start, out var length))
            {
                ExceptionHelper.ThrowInvalidOperation("Invalid buffer used for sendbuffer");
            }
            if (start != 0) ExceptionHelper.ThrowInvalidOperation("Invalid start for buffer");
            if (manager.RingId != this) ExceptionHelper.ThrowInvalidOperation($"Invalid ring id, expected {_ringId} actual {manager.RingId}");
            var hasLock = false;
            try
            {
                _lock.Enter(ref hasLock);
                _sendQueue.Enqueue((manager, (ushort)length));
            }
            finally
            {
                if (hasLock) _lock.Exit(useMemoryBarrier: true);
            }
            _sendEvent.Set();
        }

        internal override void Return(int buffer_index) => throw new NotImplementedException();
    }
}

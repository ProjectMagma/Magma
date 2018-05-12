using System;
using System.Runtime.InteropServices;
using System.Threading;
using Magma.NetMap.Interop;
using static Magma.NetMap.Interop.Netmap;

namespace Magma.NetMap
{
    public sealed class NetMapTransmitRing : NetMapRing
    {
        private const int SPINCOUNT = 100;
        private const int MAXLOOPTRY = 2;

        private readonly object _sendBufferLock = new object();

        internal unsafe NetMapTransmitRing(RxTxPair rxTxPair, byte* memoryRegion, long queueOffset)
            : base(rxTxPair, memoryRegion, queueOffset)
        {
        }

        public bool TryGetNextBuffer(out Memory<byte> buffer)
        {
            ref var ring = ref RingInfo();
            for (var loop = 0; loop < MAXLOOPTRY; loop++)
            {
                var slotIndex = GetCursor();
                if (slotIndex == -1)
                {
                    ForceFlush();   // Thread.SpinWait(SPINCOUNT);
                    continue;
                }
                var slot = GetSlot(slotIndex);
                var manager = _bufferPool.GetBuffer(slot.buf_idx);
                manager.RingId = _ringId;
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
            if (manager.RingId != _ringId) ExceptionHelper.ThrowInvalidOperation($"Invalid ring id, expected {_ringId} actual {manager.RingId}");

            lock (_sendBufferLock)
            {
                ref var ring = ref RingInfo();
                var newHead = RingNext(ring.Head);
                ref var slot = ref GetSlot(ring.Head);
                if (slot.buf_idx != manager.BufferIndex)
                {
                    slot.buf_idx = manager.BufferIndex;
                    slot.flags |= NetmapSlotFlags.NS_BUF_CHANGED;
                }
                slot.len = (ushort)buffer.Length;
                ring.Head = newHead;
            }
        }

        internal bool TrySendWithSwap(ref NetmapSlot sourceSlot)
        {
            ref var ring = ref RingInfo();
            for (var loop = 0; loop < MAXLOOPTRY; loop++)
            {
                lock (_sendBufferLock)
                {
                    var slotIndex = GetCursor();
                    if (slotIndex == -1)
                    {
                        Thread.SpinWait(SPINCOUNT);
                        continue;
                    }
                    ref var slot = ref GetSlot(slotIndex);
                    var buffIndex = slot.buf_idx;
                    slot.buf_idx = sourceSlot.buf_idx;
                    slot.len = sourceSlot.len;
                    slot.flags |= NetmapSlotFlags.NS_BUF_CHANGED;

                    sourceSlot.buf_idx = buffIndex;
                    sourceSlot.flags |= NetmapSlotFlags.NS_BUF_CHANGED;
                    ring.Head = RingNext(slotIndex);
                    return true;
                }
            }
            Console.WriteLine("Dropped packet on swap");
            return false;
        }

        public unsafe void ForceFlush() => _rxTxPair.ForceFlush();
    }
}

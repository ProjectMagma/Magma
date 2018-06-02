using System;
using System.Runtime.CompilerServices;
using System.Threading;
using Magma.NetMap.Interop;
using static Magma.NetMap.Interop.Netmap;

namespace Magma.NetMap.Internal
{
    internal unsafe abstract class NetMapRing
    {
        protected readonly byte* _memoryRegion;
        protected readonly long _queueOffset;
        protected readonly int _bufferSize;

        protected readonly int _numberOfSlots;
        protected readonly int _ringId;
        private readonly NetmapSlot* _rxRing;
        protected readonly byte* _bufferStart;
        internal RxTxPair _rxTxPair;
        internal NetMapBufferPool _bufferPool;

        protected NetMapRing(RxTxPair rxTxPair, byte* memoryRegion, long queueOffset)
        {
            _rxTxPair = rxTxPair;
            _queueOffset = queueOffset;
            _memoryRegion = memoryRegion;
            var ringInfo = RingInfo;
            _bufferSize = (int)ringInfo.BufferSize;
            _numberOfSlots = (int)ringInfo.NumberOfSlotsPerRing;
            _bufferStart = _memoryRegion + _queueOffset + ringInfo.BuffersOffset;
            _ringId = ringInfo.RingId;

            _rxRing = (NetmapSlot*)((long)(_memoryRegion + queueOffset + Unsafe.SizeOf<NetmapRing>() + 127 + 128) & (~0xFF));
        }

        internal NetMapBufferPool BufferPool { set => _bufferPool = value; }
        internal unsafe ref NetmapRing RingInfo => ref Unsafe.AsRef<NetmapRing>((_memoryRegion + _queueOffset));

        internal Span<byte> GetBuffer(uint bufferIndex) => GetBuffer(bufferIndex, (ushort)_bufferSize);
        internal ref NetmapSlot GetSlot(int index) => ref _rxRing[index];
        internal unsafe IntPtr BufferStart => (IntPtr)_bufferStart;

        internal int BufferSize => _bufferSize;

        internal Span<byte> GetBuffer(uint bufferIndex, ushort size)
        {
            var ptr = _bufferStart + (bufferIndex * _bufferSize);
            return new Span<byte>(ptr, size);
        }

        internal int RingNext(int i) => (i + 1 == _numberOfSlots) ? 0 : i + 1;
        internal bool IsRingEmpty()
        {
            ref var ring = ref RingInfo;
            return (Volatile.Read(ref ring.Cursor) == Volatile.Read(ref ring.Tail));
        }

        internal int GetCursor()
        {
            ref var ring = ref RingInfo;
            var cursor = ring.Cursor;
            if (RingSpace(cursor) > 0)
            {
                var newIndex = RingNext(cursor);
                if (Interlocked.CompareExchange(ref ring.Cursor, newIndex, cursor) == cursor)
                {
                    return cursor;
                }
            }
            else
            {
                //No space so we will spin or backpressure
                return -1;
            }
        }

        public int RingSpace(int cursor)
        {
            ref var ring = ref RingInfo;
            var ret = ring.Tail - cursor;
            if (ret < 0)
                ret += _numberOfSlots;
            return ret;
        }

        internal uint GetMaxBufferId()
        {
            var max = 0u;
            for (var i = 0; i < _numberOfSlots; i++)
            {
                max = Math.Max(max, _rxRing[i].buf_idx);
            }
            return max;
        }

        internal abstract void Return(int buffer_index);
        public abstract void Start();
    }
}

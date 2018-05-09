using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using Magma.NetMap.Interop;

namespace Magma.NetMap
{
    public unsafe abstract class NetMapRing
    {
        protected readonly byte* _memoryRegion;
        protected readonly long _queueOffset;
        protected readonly int _bufferSize;
        protected readonly int _numberOfSlots;
        protected readonly int _ringId;
        protected readonly Netmap_slot* _rxRing;
        protected readonly byte* _bufferStart;
        protected int _fileDescriptor;
        protected NetMapBufferPool _bufferPool;

        protected NetMapRing(byte* memoryRegion, ulong rxQueueOffset)
        {
            _queueOffset = (long)rxQueueOffset;
            _memoryRegion = memoryRegion;
            var ringInfo = RingInfo[0];
            _bufferSize = (int)ringInfo.nr_buf_size;
            _numberOfSlots = (int)ringInfo.num_slots;
            _bufferStart = _memoryRegion + _queueOffset + ringInfo.buf_ofs;
            _ringId = ringInfo.ringid & (ushort)nr_ringid.NETMAP_RING_MASK;

            _rxRing = (Netmap_slot*)((long)(_memoryRegion + rxQueueOffset + Unsafe.SizeOf<Netmap_ring>() + 127 + 128) & (~0xFF));
        }

        internal NetMapBufferPool BufferPool { set => _bufferPool = value; }
        internal unsafe Netmap_ring* RingInfo => (Netmap_ring*)(_memoryRegion + _queueOffset);

        internal Span<byte> GetBuffer(uint bufferIndex) => GetBuffer(bufferIndex, (ushort)_bufferSize);

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
            var ring = RingInfo[0];
            return (ring.cur == ring.tail);
        }

        internal int GetCursor()
        {
            ref var ring = ref RingInfo[0];
            while (true)
            {
                var cursor = ring.cur;
                if(RingSpace(cursor) > 0)
                {
                    var newIndex = RingNext(cursor);
                    if(Interlocked.CompareExchange(ref ring.cur, newIndex, cursor) == cursor)
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
        }

        public int RingSpace(int cursor)
        {
            var ring = RingInfo[0];
            var ret = (int)ring.tail - cursor;
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
    }
}

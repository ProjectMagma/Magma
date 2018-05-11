using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using Magma.NetMap.Interop;
using static Magma.NetMap.Interop.Libc;
using static Magma.NetMap.Interop.Netmap;

namespace Magma.NetMap
{
    public unsafe abstract class NetMapRing
    {
        protected readonly byte* _memoryRegion;
        protected readonly long _queueOffset;
        protected readonly int _bufferSize;
        protected readonly int _numberOfSlots;
        protected readonly int _ringId;
        private readonly NetmapSlot* _rxRing;
        protected readonly byte* _bufferStart;
        internal FileDescriptor _fileDescriptor;
        protected NetMapBufferPool _bufferPool;

        protected NetMapRing(string interfaceName, bool isTxRing, bool isHost, byte* memoryRegion, ulong rxQueueOffset)
        {
            _queueOffset = (long)rxQueueOffset;
            _memoryRegion = memoryRegion;
            var ringInfo = RingInfo();
            _bufferSize = (int)ringInfo.BufferSize;
            _numberOfSlots = (int)ringInfo.NumberOfSlotsPerRing;
            _bufferStart = _memoryRegion + _queueOffset + ringInfo.BuffersOffset;
            _ringId = ringInfo.RingId & (ushort)nr_ringid.NETMAP_RING_MASK;

            _rxRing = (NetmapSlot*)((long)(_memoryRegion + rxQueueOffset + Unsafe.SizeOf<NetmapRing>() + 127 + 128) & (~0xFF));

            _fileDescriptor = Open("/dev/netmap", OpenFlags.O_RDWR);
            if (!_fileDescriptor.IsValid) throw new InvalidOperationException($"Need to handle properly (release memory etc) error was {_fileDescriptor}");
            var request = new NetMapRequest
            {
                nr_cmd = 0,
                nr_ringid = (ushort)_ringId,
                nr_version = Consts.NETMAP_API,
            };
            if (isHost)
            {
                request.nr_flags = NetMapRequestFlags.NR_REG_SW;
            }
            else
            {
                request.nr_flags = NetMapRequestFlags.NR_REG_ONE_NIC;
            }
            request.nr_flags |= (isTxRing ? NetMapRequestFlags.NR_TX_RINGS_ONLY : NetMapRequestFlags.NR_RX_RINGS_ONLY);

            var textbytes = Encoding.ASCII.GetBytes(interfaceName + "\0");
            fixed (void* txtPtr = textbytes)
            {
                Buffer.MemoryCopy(txtPtr, request.nr_name, textbytes.Length, textbytes.Length);
            }

            if (IOCtl(_fileDescriptor, IOControlCommand.NIOCREGIF, ref request) != 0) ExceptionHelper.ThrowInvalidOperation("Failed to open an FD for a single ring");
        }

        internal NetMapBufferPool BufferPool { set => _bufferPool = value; }
        internal unsafe ref NetmapRing RingInfo() => ref Unsafe.AsRef<NetmapRing>((_memoryRegion + _queueOffset));

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
            var ring = RingInfo();
            return (ring.Cursor == ring.Tail);
        }

        internal int GetCursor()
        {
            ref var ring = ref RingInfo();
            while (true)
            {
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
        }

        public int RingSpace(int cursor)
        {
            var ring = RingInfo();
            var ret = (int)ring.Tail - cursor;
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

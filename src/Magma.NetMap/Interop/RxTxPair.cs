using System;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using static Magma.NetMap.Interop.Libc;
using static Magma.NetMap.Interop.Netmap;

namespace Magma.NetMap.Interop
{
    internal unsafe class NetMapRing : IDisposable
    {
        private FileDescriptor _fileDescriptor;
        private readonly int _ringId;
        private readonly bool _isHostStack;
        private readonly bool _isTxRing;
        private readonly byte* _memoryRegion;
        private long _queueOffset;
        private readonly int _bufferSize;
        private readonly byte* _bufferStart;
        private readonly int _numberOfSlots;
        private readonly NetmapSlot* _ringArray;

        internal NetMapRing(string interfaceName, bool isHostStack, bool isTxRing,
            byte* memoryRegion, long queueOffset)
        {
            _queueOffset = queueOffset;
            _memoryRegion = memoryRegion;
            _isTxRing = isTxRing;
            _isHostStack = isHostStack;
            var ringInfo = RingInfo;
            _ringId = ringInfo.RingId;
            _bufferSize = (int)ringInfo.BufferSize;
            _bufferStart = _memoryRegion + _queueOffset + ringInfo.BuffersOffset;
            _numberOfSlots = (int)ringInfo.NumberOfSlotsPerRing;
            _ringArray = (NetmapSlot*)((long)(_memoryRegion + queueOffset + Unsafe.SizeOf<NetmapRing>() + 127 + 128) & (~0xFF));

            var flags = isHostStack ? NetMapRequestFlags.NR_REG_SW : NetMapRequestFlags.NR_REG_ONE_NIC;
            if (isTxRing)
            {
                flags |= NetMapRequestFlags.NR_TX_RINGS_ONLY;
            }
            else
            {
                flags |= NetMapRequestFlags.NR_RX_RINGS_ONLY & NetMapRequestFlags.NR_NO_TX_POLL;
            }
            _ringId = isHostStack ? 0 : _ringId;

            _fileDescriptor = OpenNetMap(interfaceName, _ringId, flags, out var request);
        }

        public int BufferSize => _bufferSize;

        public void WaitForWork()
        {
            var pfd = new PollFileDescriptor()
            {
                Events = PollEvents.POLLIN,
                Fd = _fileDescriptor,
            };
            var result = Poll(ref pfd, 1, -1);
            if (result < 0) ExceptionHelper.ThrowInvalidOperation("Error on poll");
        }

        public void ForceFlush() => IOCtl(_fileDescriptor, IOControlCommand.NIOCTXSYNC, IntPtr.Zero);

        public ref NetmapRing RingInfo => ref Unsafe.AsRef<NetmapRing>((_memoryRegion + _queueOffset));
        public Span<byte> GetBuffer(uint bufferIndex) => GetBuffer(bufferIndex, (ushort)_bufferSize);
        public IntPtr BufferStart => (IntPtr)_bufferStart;

        public Span<byte> GetBuffer(uint bufferIndex, ushort size)
        {
            var ptr = _bufferStart + (bufferIndex * _bufferSize);
            return new Span<byte>(ptr, size);
        }

        public int RingNext(int i) => (i + 1 == _numberOfSlots) ? 0 : i + 1;

        public bool IsRingEmpty()
        {
            ref var ring = ref RingInfo;
            return (Volatile.Read(ref ring.Cursor) == Volatile.Read(ref ring.Tail));
        }

        public uint GetMaxBufferId()
        {
            var max = 0u;
            for (var i = 0; i < _numberOfSlots; i++)
            {
                max = Math.Max(max, _ringArray[i].buf_idx);
            }
            return max;
        }

        public int RingSpace(int cursor)
        {
            ref var ring = ref RingInfo;
            var ret = ring.Tail - cursor;
            if (ret < 0)
                ret += _numberOfSlots;
            return ret;
        }

        public int GetCursor()
        {
            ref var ring = ref RingInfo;
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

        public ref NetmapSlot GetSlot(int index) => ref _ringArray[index];

        public void Dispose()
        {
            if (_fileDescriptor.IsValid)
            {
                Close(_fileDescriptor);
                _fileDescriptor = new FileDescriptor();
            }
        }
    }
}

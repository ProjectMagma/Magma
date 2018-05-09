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

        protected NetMapRing(string interfaceName, bool isTxRing, bool isHost, byte* memoryRegion, ulong rxQueueOffset)
        {
            _queueOffset = (long)rxQueueOffset;
            _memoryRegion = memoryRegion;
            var ringInfo = RingInfo[0];
            _bufferSize = (int)ringInfo.nr_buf_size;
            _numberOfSlots = (int)ringInfo.num_slots;
            _bufferStart = _memoryRegion + _queueOffset + ringInfo.buf_ofs;
            _ringId = ringInfo.ringid & (ushort)nr_ringid.NETMAP_RING_MASK;

            _rxRing = (Netmap_slot*)((long)(_memoryRegion + rxQueueOffset + Unsafe.SizeOf<Netmap_ring>() + 127 + 128) & (~0xFF));

            _fileDescriptor = Unix.Open("/dev/netmap", Unix.OpenFlags.O_RDWR);
            if (_fileDescriptor < 0) throw new InvalidOperationException($"Need to handle properly (release memory etc) error was {_fileDescriptor}");
            var request = new NetMapRequest
            {
                nr_cmd = 0,
                nr_ringid = (ushort)_ringId,
                nr_version = Consts.NETMAP_API,
            };
            if(isHost)
            {
                request.nr_flags = (uint)NetMapRequestMode.NR_REG_SW;
            }
            else
            {
                request.nr_flags = (uint)NetMapRequestMode.NR_REG_ONE_NIC | (isTxRing ? (uint)NetMapRequestFlags.NR_TX_RINGS_ONLY : (uint)NetMapRequestFlags.NR_RX_RINGS_ONLY);
            }

            Console.WriteLine($"Getting FD for Receive RingID {_ringId}");
            var textbytes = Encoding.ASCII.GetBytes(interfaceName + "\0");
            fixed (void* txtPtr = textbytes)
            {
                Buffer.MemoryCopy(txtPtr, request.nr_name, textbytes.Length, textbytes.Length);
            }
            
            if (Unix.IOCtl(_fileDescriptor, Consts.NIOCREGIF, &request) != 0) throw new InvalidOperationException("Failed to open an FD for a single ring");
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
                if (RingSpace(cursor) > 0)
                {
                    var newIndex = RingNext(cursor);
                    if (Interlocked.CompareExchange(ref ring.cur, newIndex, cursor) == cursor)
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

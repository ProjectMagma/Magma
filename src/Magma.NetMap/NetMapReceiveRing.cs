using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using Magma.NetMap.Interop;

namespace Magma.NetMap
{
    public unsafe class NetMapReceiveRing
    {
        private readonly byte* _memoryRegion;
        private readonly long _queueOffset;
        private readonly int _bufferSize;
        private readonly int _numberOfSlots;
        private readonly int _ringId;
        private readonly Thread _worker;
        private readonly Netmap_slot* _rxRing;
        private readonly byte* _bufferStart;
        private readonly int _fileDescriptor;

        internal NetMapReceiveRing(byte* memoryRegion, ulong rxQueueOffset, int fileDescriptor)
        {
            _fileDescriptor = fileDescriptor;
            _queueOffset = (long)rxQueueOffset;
            _memoryRegion = memoryRegion;

            var ringInfo = RxRingInfo[0];
            Console.WriteLine($"Ring direction {ringInfo.dir}");
            //if (ringInfo.dir != netmap_ringdirection.rx) throw new InvalidOperationException("Need better error message");
            _bufferSize = (int)ringInfo.nr_buf_size;
            _numberOfSlots = (int)ringInfo.num_slots;
            _bufferStart = _memoryRegion + _queueOffset + ringInfo.buf_ofs;
            _ringId = ringInfo.ringid & (ushort)nr_ringid.NETMAP_RING_MASK;

            _rxRing = (Netmap_slot*)((long)(_memoryRegion + rxQueueOffset + Unsafe.SizeOf<Netmap_ring>() + 127 + 128) & (~0xFF));

            Console.WriteLine($"Ring Id {_ringId} buffer offset {(long)_bufferStart} is hardware ring {(ringInfo.ringid & (short)nr_ringid.NETMAP_HW_RING) != 0} number of slots {_numberOfSlots} and buffer size {_bufferSize}");

            PrintSlotInfo(0);
            PrintSlotInfo(1);

            _worker = new Thread(new ThreadStart(ThreadLoop));
        }

        private void ThreadLoop()
        {
            while (true)
            {
                var fd = new Unix.pollFd()
                {
                    Events = Unix.PollEvents.POLLIN,
                    Fd = _fileDescriptor
                };

                var pollResult = Unix.poll(ref fd, 1, 1);
                if(pollResult < 0)
                {
                    Console.WriteLine($"Poll failed on ring {_ringId} exiting polling loop");
                    return;
                }
                Console.WriteLine($"Poll on ring {_ringId} triggered");

                var ring = RxRingInfo[0];
                while (!IsRingEmpty())
                {
                    var i = ring.cur;
                    var slot = _rxRing[i];
                    var buffer = GetBuffer(slot.buf_idx);
                    Console.WriteLine($"Received packet on ring {_ringId} data was {BitConverter.ToString(buffer.ToArray())}");
                    ring.head = ring.cur = RingNext(i);
                }
            }
        }

        private uint RingNext(uint i) => (i + 1 == _numberOfSlots) ? 0 : i + 1;

        private void PrintSlotInfo(int index)
        {
            var slot = _rxRing[index];
            Console.WriteLine($"Slot {index} bufferIndex {slot.buf_idx} flags {slot.flags} length {slot.len} pointer {slot.ptr}");
        }

        private Span<byte> GetBuffer(uint bufferIndex)
        {
            var ptr = _bufferStart + (bufferIndex * _bufferSize);
            return new Span<byte>(ptr, _bufferSize);
        }

        private bool IsRingEmpty()
        {
            var ring = RxRingInfo[0];
            return (ring.cur == ring.tail);
        }

        private Netmap_slot CurrentSlot => _rxRing[0];

        private unsafe Netmap_ring* RxRingInfo => (Netmap_ring*)(_memoryRegion + _queueOffset);
    }
}

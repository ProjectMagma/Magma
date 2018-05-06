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
        private readonly long _bufferStart;

        internal NetMapReceiveRing(byte* memoryRegion, ulong rxQueueOffset)
        {
            _queueOffset = (long)rxQueueOffset;
            _memoryRegion = memoryRegion;

            var ringInfo = RxRingInfo[0];
            Console.WriteLine($"Ring direction {ringInfo.dir}");
            //if (ringInfo.dir != netmap_ringdirection.rx) throw new InvalidOperationException("Need better error message");
            _bufferSize = (int)ringInfo.nr_buf_size;
            _numberOfSlots = (int)ringInfo.num_slots;
            _bufferStart = _queueOffset + ringInfo.buf_ofs;
            _ringId = ringInfo.ringid & (ushort)nr_ringid.NETMAP_RING_MASK;

            _rxRing = (Netmap_slot*)((long)(_memoryRegion + rxQueueOffset + Unsafe.SizeOf<Netmap_ring>() + 127 + 128) & (~0xFF));

            Console.WriteLine($"Ring Id {_ringId} buffer offset {(long)_bufferStart} is hardware ring {(ringInfo.ringid & (short)nr_ringid.NETMAP_HW_RING) != 0} number of slots {_numberOfSlots} and buffer size {_bufferSize}");

            PrintSlotInfo(0);
            PrintSlotInfo(1);
        }

        private void PrintSlotInfo(int index)
        {
            var slot = _rxRing[index];
            Console.WriteLine($"Slot {index} bufferIndex {slot.buf_idx} flags {slot.flags} length {slot.len} pointer {slot.ptr}");
        }

        private Netmap_slot CurrentSlot => _rxRing[0];

        private unsafe Netmap_ring* RxRingInfo => (Netmap_ring*)(_memoryRegion + _queueOffset);
    }
}

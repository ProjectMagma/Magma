using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using Magma.NetMap.Interop;

namespace Magma.NetMap
{
    public unsafe class NetMapRingPair
    {
        private byte* _memoryRegion;
        private int _rxQueueOffset;
        private int _rxBufferSize;
        private int _rxSlots;
        private int _ringId;
        private Thread _worker;
        private netmap_slot* _rxRing;
        
        internal NetMapRingPair(byte* memoryRegion, ulong txQueueOffset, ulong rxQueueOffset)
        {
            _rxQueueOffset = (int) rxQueueOffset;
            _memoryRegion = memoryRegion;

            var ringInfo = RxRingInfo[0];
            Console.WriteLine($"Ring direction {ringInfo.dir}");
            //if (ringInfo.dir != netmap_ringdirection.rx) throw new InvalidOperationException("Need better error message");
            _rxBufferSize = (int)ringInfo.nr_buf_size;
            _rxSlots = (int)ringInfo.num_slots;
            _ringId = ringInfo.ringid & (ushort)nr_ringid.NETMAP_RING_MASK;

            _rxRing =(netmap_slot*)( (long)(_memoryRegion + rxQueueOffset + Unsafe.SizeOf<netmap_ring>() + 15) & (~0x0F));

            Console.WriteLine($"Ring Id {_ringId} is hardware ring {(ringInfo.ringid & (short)nr_ringid.NETMAP_HW_RING) != 0} number of slots {_rxSlots} and buffer size {_rxBufferSize}");
            var span = new Span<byte>((_memoryRegion + rxQueueOffset), 500);
            Console.WriteLine($"500 bytes from the start of the ring {BitConverter.ToString(span.ToArray())}");
            PrintSlotInfo(0);
            PrintSlotInfo(1);
        }

        private void PrintSlotInfo(int index)
        {
            var slot = _rxRing[index];
            Console.WriteLine($"Slot {index} bufferIndex {slot.buf_idx} flags {slot.flags} length {slot.len} pointer {slot.ptr}");
        }

        private netmap_slot CurrentSlot => _rxRing[0];

        private unsafe netmap_ring* RxRingInfo => (netmap_ring*)(_memoryRegion + _rxQueueOffset);

        public void Send(Span<byte> packet)
        {

        }
    }
}

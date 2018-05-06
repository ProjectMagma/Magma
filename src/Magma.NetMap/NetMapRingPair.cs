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
        
        internal NetMapRingPair(byte* memoryRegion, ulong rxQueueOffset, ulong txQueueOffset)
        {
            _rxQueueOffset = (int) rxQueueOffset;
            _memoryRegion = memoryRegion;

            var ringInfo = RxRingInfo[0];
            if (ringInfo.dir != netmap_ringdirection.rx) throw new InvalidOperationException("Need better error message");
            _rxBufferSize = (int)ringInfo.nr_buf_size;
            _rxSlots = (int)ringInfo.num_slots;
            _ringId = ringInfo.ringid & (ushort)nr_ringid.NETMAP_RING_MASK;

            Console.WriteLine($"Ring Id {_ringId} is hardware ring {(ringInfo.ringid & (short)nr_ringid.NETMAP_HW_RING) != 0} number of slots {_rxSlots} and buffer size {_rxBufferSize}");
        }

        private unsafe netmap_ring* RxRingInfo => (netmap_ring*)(_memoryRegion + _rxQueueOffset);

        public void Send(Span<byte> packet)
        {

        }
    }
}

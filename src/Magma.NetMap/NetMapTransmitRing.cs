using System;
using System.Collections.Generic;
using System.Text;
using Magma.NetMap.Interop;

namespace Magma.NetMap
{
    public unsafe class NetMapTransmitRing
    {
        private readonly byte* _memoryRegion;
        private readonly long _queueOffset;
        private readonly int _bufferSize;
        private readonly int _numberOfSlots;
        private readonly int _ringId;
        private readonly Netmap_slot* _rxRing;
        private readonly byte* _bufferStart;
        private readonly int _fileDescriptor;

        internal NetMapTransmitRing(byte* memoryRegion, ulong rxQueueOffset, int fileDescriptor)
        {
            _fileDescriptor = fileDescriptor;
            _queueOffset = (long)rxQueueOffset;
            _memoryRegion = memoryRegion;

            var ringInfo = RxRingInfo[0];
            Console.WriteLine($"Ring direction {ringInfo.dir}");
        }

        private unsafe Netmap_ring* RxRingInfo => (Netmap_ring*)(_memoryRegion + _queueOffset);
    }
}

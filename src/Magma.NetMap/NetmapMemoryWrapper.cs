using System;
using System.Buffers;
using System.Collections.Generic;
using System.Text;

namespace Magma.NetMap
{
    internal struct NetmapMemoryWrapper : IMemoryOwner<byte>
    {
        private NetMapOwnedMemory _ownedMemory;

        internal NetmapMemoryWrapper(NetMapOwnedMemory ownedMemory) => _ownedMemory = ownedMemory;

        public Memory<byte> Memory => _ownedMemory.Memory;

        public bool IsEmpty => _ownedMemory == null;

        public void Dispose() => _ownedMemory.Return();
    }
}

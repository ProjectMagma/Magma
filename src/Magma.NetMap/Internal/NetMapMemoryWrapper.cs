using System;
using System.Buffers;

namespace Magma.NetMap.Internal
{
    internal struct NetMapMemoryWrapper : IMemoryOwner<byte>
    {
        private NetMapOwnedMemory _ownedMemory;

        public NetMapMemoryWrapper(NetMapOwnedMemory ownedMemory) => _ownedMemory = ownedMemory;

        public Memory<byte> Memory => _ownedMemory.Memory;

        public void Dispose() => _ownedMemory.Return();

        public bool IsEmpty => _ownedMemory == null;
    }
}

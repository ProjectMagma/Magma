using System;
using System.Collections.Generic;
using System.Text;

namespace Magma.NetMap.Internal
{
    internal class NetMapBufferPool
    {
        private readonly NetMapOwnedMemory[] _memoryPool;

        public NetMapBufferPool(ushort bufferLength, IntPtr bufferStart, uint numberOfBuffers)
        {
            _memoryPool = new NetMapOwnedMemory[numberOfBuffers];
            for(var i = 0; i < numberOfBuffers;i++)
            {
                _memoryPool[i] = new NetMapOwnedMemory(IntPtr.Add(bufferStart, i * bufferLength), bufferLength, (uint)i);
            }
        }

        public NetMapOwnedMemory GetBuffer(uint bufferIndex) => _memoryPool[bufferIndex];
    }
}

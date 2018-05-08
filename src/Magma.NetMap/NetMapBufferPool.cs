using System;
using System.Collections.Generic;
using System.Text;

namespace Magma.NetMap
{
    public class NetMapBufferPool
    {
        private NetMapOwnedMemory[] _memoryPool;

        public unsafe NetMapBufferPool(ushort bufferLength, IntPtr bufferStart, uint numberOfBuffers)
        {
            _memoryPool = new NetMapOwnedMemory[numberOfBuffers];
            var ptr = (byte*)bufferStart.ToPointer();
            for(var i = 0; i < numberOfBuffers;i++)
            {
                _memoryPool[i] = new NetMapOwnedMemory(IntPtr.Add(bufferStart, i * bufferLength), bufferLength, (uint)i);
            }
        }

        public NetMapOwnedMemory GetBuffer(uint bufferIndex) => _memoryPool[bufferIndex];
    }
}

using System;
using System.Buffers;
using System.Collections.Generic;
using System.Text;

namespace Magma.NetMap
{
    public class NetMapOwnedMemory : MemoryManager<byte>
    {
        private IntPtr _pointer;
        private ushort _length;
        private uint _bufferIndex;

        public NetMapOwnedMemory(IntPtr pointer, ushort length, uint index)
        {
            _pointer = pointer;
            _length = length;
            _bufferIndex = index;
        }

        public uint BufferIndex => _bufferIndex;

        public unsafe override Span<byte> GetSpan() => new Span<byte>(_pointer.ToPointer(), _length);

        public unsafe override MemoryHandle Pin(int elementIndex = 0) => new MemoryHandle(IntPtr.Add(_pointer, elementIndex).ToPointer());
       
        public override void Unpin() { }

        protected override void Dispose(bool disposing) { }
    }
}

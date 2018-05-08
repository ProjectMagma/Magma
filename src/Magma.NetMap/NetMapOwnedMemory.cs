using System;
using System.Buffers;

namespace Magma.NetMap
{
    public class NetMapOwnedMemory : MemoryManager<byte>
    {
        private IntPtr _pointer;
        private ushort _length;

        public NetMapOwnedMemory(IntPtr pointer, ushort length, uint index)
        {
            _pointer = pointer;
            _length = length;
            BufferIndex = index;
        }

        public uint BufferIndex { get; }

        public override Memory<byte> Memory => CreateMemory(_length);

        public unsafe override Span<byte> GetSpan() => new Span<byte>(_pointer.ToPointer(), _length);

        public unsafe override MemoryHandle Pin(int elementIndex = 0) => new MemoryHandle(IntPtr.Add(_pointer, elementIndex).ToPointer());
       
        public override void Unpin() { }

        protected override void Dispose(bool disposing) { }
    }
}

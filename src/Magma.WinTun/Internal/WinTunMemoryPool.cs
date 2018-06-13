using System;
using System.Buffers;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Magma.WinTun.Internal
{
    public class WinTunMemoryPool
    {
        private Queue<WinTunOwnedMemory> _pool = new Queue<WinTunOwnedMemory>();
        private AsyncEventHandle _emptyEvent = new AsyncEventHandle();

        public unsafe WinTunMemoryPool(int poolSize, int bufferSize)
        {
            var totalMemory = poolSize * bufferSize;
            var memory = Marshal.AllocHGlobal(totalMemory);
            for(var i = 0; i < totalMemory;i+=bufferSize)
            {
                var mem = new WinTunOwnedMemory(this, memory, i, bufferSize);
                _pool.Enqueue(mem);
            }
        }

        public bool TryGetMemory(out WinTunOwnedMemory memory)
        {
            lock(_pool)
            {
                var result = _pool.TryDequeue(out memory);
                if (!result) _emptyEvent.Reset();
                return result;
            }
        }

        private void Return(WinTunOwnedMemory ownedMemory)
        {
            lock(_pool)
            {
                _pool.Enqueue(ownedMemory);
                _emptyEvent.Set();
            }
        }

        public async Task<WinTunOwnedMemory> GetMemoryAsync()
        {
            while(true)
            {
                lock(_pool)
                {
                    var result = TryGetMemory(out var returnMemory);
                    if (result) return returnMemory;
                }
                await _emptyEvent.WaitAsync();
            }
        }

        public unsafe class WinTunOwnedMemory : MemoryManager<byte>
        {
            private byte* _memoryPtr;
            private int _length;
            private WinTunMemoryPool _pool;

            public WinTunOwnedMemory(WinTunMemoryPool memoryPool, IntPtr memoryPtr, int startIndex, int length)
            {
                _pool = memoryPool;
                _memoryPtr = (byte*)memoryPtr.ToPointer() + startIndex;
                _length = length;
            }

            public unsafe override Span<byte> GetSpan() => new Span<byte>(_memoryPtr, _length);

            public override MemoryHandle Pin(int elementIndex = 0) => new MemoryHandle(_memoryPtr + elementIndex);


            public override Memory<byte> Memory => CreateMemory(_length);

            public override void Unpin() { }

            protected override void Dispose(bool disposing) => _pool.Return(this);

            public void Return() => _pool.Return(this);
        }
    }
}

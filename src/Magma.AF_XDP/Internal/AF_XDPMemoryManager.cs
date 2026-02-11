using System;
using System.Buffers;
using System.Runtime.InteropServices;
using Magma.AF_XDP.Interop;
using static Magma.AF_XDP.Interop.LibBpf;

namespace Magma.AF_XDP.Internal
{
    /// <summary>
    /// Manages UMEM (User Memory) for AF_XDP sockets
    /// </summary>
    public class AF_XDPMemoryManager : IDisposable
    {
        private readonly IntPtr _umemArea;
        private readonly ulong _umemSize;
        private readonly uint _frameSize;
        private readonly uint _frameCount;
        private IntPtr _umem;
        private xsk_ring_prod _fillRing;
        private xsk_ring_cons _compRing;
        private bool _disposed;

        public AF_XDPMemoryManager(uint frameCount, uint frameSize)
        {
            _frameCount = frameCount;
            _frameSize = frameSize;
            _umemSize = (ulong)frameCount * frameSize;

            // Allocate UMEM area
            _umemArea = Marshal.AllocHGlobal((int)_umemSize);
            if (_umemArea == IntPtr.Zero)
                throw new OutOfMemoryException("Failed to allocate UMEM area");

            // Initialize UMEM with libbpf
            var config = new xsk_umem_config
            {
                fill_size = frameCount / 2,
                comp_size = frameCount / 2,
                frame_size = frameSize,
                frame_headroom = 0,
                flags = 0
            };

            int ret = xsk_umem__create(out _umem, _umemArea, _umemSize, ref _fillRing, ref _compRing, ref config);
            if (ret != 0)
            {
                Marshal.FreeHGlobal(_umemArea);
                throw new InvalidOperationException($"Failed to create UMEM: error code {ret}");
            }

            // Pre-populate fill ring with all frames
            PopulateFillRing();
        }

        public IntPtr Umem => _umem;
        public ref xsk_ring_prod FillRing => ref _fillRing;
        public ref xsk_ring_cons CompRing => ref _compRing;
        public uint FrameSize => _frameSize;

        public IntPtr GetFrameAddress(ulong frameIndex)
        {
            return IntPtr.Add(_umemArea, (int)(frameIndex * _frameSize));
        }

        private unsafe void PopulateFillRing()
        {
            uint idx;
            uint reserved = xsk_ring_prod__reserve(ref _fillRing, _frameCount, out idx);
            
            for (uint i = 0; i < reserved; i++)
            {
                IntPtr addrPtr = xsk_ring_prod__fill_addr(ref _fillRing, idx + i);
                *(ulong*)addrPtr.ToPointer() = i * _frameSize;
            }

            xsk_ring_prod__submit(ref _fillRing, reserved);
        }

        public unsafe Memory<byte> GetFrameMemory(ulong frameAddr)
        {
            IntPtr ptr = IntPtr.Add(_umemArea, (int)frameAddr);
            return new UnmanagedMemoryManager<byte>((byte*)ptr.ToPointer(), (int)_frameSize).Memory;
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                if (_umem != IntPtr.Zero)
                {
                    xsk_umem__delete(_umem);
                    _umem = IntPtr.Zero;
                }

                if (_umemArea != IntPtr.Zero)
                {
                    Marshal.FreeHGlobal(_umemArea);
                }

                _disposed = true;
            }
        }
    }

    /// <summary>
    /// Memory manager for unmanaged memory regions
    /// </summary>
    internal unsafe class UnmanagedMemoryManager<T> : MemoryManager<T> where T : unmanaged
    {
        private readonly T* _pointer;
        private readonly int _length;

        public UnmanagedMemoryManager(T* pointer, int length)
        {
            _pointer = pointer;
            _length = length;
        }

        public override Span<T> GetSpan() => new Span<T>(_pointer, _length);

        public override MemoryHandle Pin(int elementIndex = 0)
        {
            if (elementIndex < 0 || elementIndex >= _length)
                throw new ArgumentOutOfRangeException(nameof(elementIndex));

            return new MemoryHandle(_pointer + elementIndex);
        }

        public override void Unpin() { }

        protected override void Dispose(bool disposing) { }
    }
}

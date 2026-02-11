using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Magma.AF_XDP.Interop;
using Magma.Network.Abstractions;
using static Magma.AF_XDP.Interop.LibBpf;

namespace Magma.AF_XDP.Internal
{
    /// <summary>
    /// AF_XDP transmit ring for sending packets via XDP socket
    /// Implements IPacketTransmitter for integration with Magma transport layer
    /// </summary>
    public class AF_XDPTransmitRing : IPacketTransmitter
    {
        private readonly IntPtr _xskSocket;
        private readonly int _socketFd;
        private readonly AF_XDPMemoryManager _memoryManager;
        private readonly string _interfaceName;
        private readonly Random _random = new Random();
        private xsk_ring_prod _txRing;
        private bool _disposed;
        private ulong _currentFrameAddr;

        public AF_XDPTransmitRing(IntPtr xskSocket, AF_XDPMemoryManager memoryManager, ref xsk_ring_prod txRing, string interfaceName)
        {
            _xskSocket = xskSocket;
            _memoryManager = memoryManager ?? throw new ArgumentNullException(nameof(memoryManager));
            _txRing = txRing;
            _interfaceName = interfaceName ?? throw new ArgumentNullException(nameof(interfaceName));
            _socketFd = xsk_socket__fd(xskSocket);
        }

        public bool TryGetNextBuffer(out Memory<byte> buffer)
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(AF_XDPTransmitRing));

            // Reserve a slot in the TX ring
            uint idx;
            uint reserved = xsk_ring_prod__reserve(ref _txRing, 1, out idx);
            
            if (reserved == 0)
            {
                buffer = default;
                return false;
            }

            // Get TX descriptor for this slot
            IntPtr descPtr = xsk_ring_prod__tx_desc(ref _txRing, idx);
            if (descPtr == IntPtr.Zero)
            {
                buffer = default;
                return false;
            }

            // Allocate a frame from UMEM (simplified - in production would need frame pool management)
            _currentFrameAddr = idx * _memoryManager.FrameSize;
            
            // Get memory for this frame
            buffer = _memoryManager.GetFrameMemory(_currentFrameAddr);
            return true;
        }

        public unsafe Task SendBuffer(ReadOnlyMemory<byte> buffer)
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(AF_XDPTransmitRing));

            // Reserve a slot in the TX ring
            uint idx;
            uint reserved = xsk_ring_prod__reserve(ref _txRing, 1, out idx);
            
            if (reserved == 0)
            {
                throw new InvalidOperationException("Failed to reserve TX ring slot");
            }

            // Get TX descriptor for this slot
            IntPtr descPtr = xsk_ring_prod__tx_desc(ref _txRing, idx);
            xdp_desc* desc = (xdp_desc*)descPtr.ToPointer();

            // Set descriptor fields
            desc->addr = _currentFrameAddr;
            desc->len = (uint)buffer.Length;
            desc->options = 0;

            // Submit the packet
            xsk_ring_prod__submit(ref _txRing, 1);

            // Wake up kernel if needed
            if (xsk_ring_prod__needs_wakeup(ref _txRing))
            {
                sendto(_socketFd, IntPtr.Zero, 0, MSG_DONTWAIT, IntPtr.Zero, 0);
            }

            return Task.CompletedTask;
        }

        public uint RandomSequenceNumber()
        {
            return (uint)_random.Next();
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                // Socket cleanup is handled by AF_XDPPort
                _disposed = true;
            }
        }
    }
}

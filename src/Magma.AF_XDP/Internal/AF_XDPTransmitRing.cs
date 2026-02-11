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
        private readonly nint _xskSocket;
        private readonly int _socketFd;
        private readonly AF_XDPMemoryManager _memoryManager;
        private readonly string _interfaceName;
        private readonly Random _random = new Random();
        private xsk_ring_prod _txRing;
        private bool _disposed;
        private ulong _currentFrameAddr;
        private uint _reservedIdx;
        private bool _hasReservedSlot;

        public AF_XDPTransmitRing(nint xskSocket, AF_XDPMemoryManager memoryManager, ref xsk_ring_prod txRing, string interfaceName)
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

            // If already have a reserved slot, clean it up first
            if (_hasReservedSlot)
            {
                buffer = default;
                return false;
            }

            // Reserve a slot in the TX ring
            uint idx;
            uint reserved = xsk_ring_prod__reserve(ref _txRing, 1, out idx);
            
            if (reserved == 0)
            {
                buffer = default;
                return false;
            }

            _reservedIdx = idx;
            _hasReservedSlot = true;

            // Allocate a frame from UMEM (using ring index as simple allocator)
            // TODO: In production, implement proper frame pool management
            _currentFrameAddr = idx * _memoryManager.FrameSize;
            
            // Get memory for this frame
            buffer = _memoryManager.GetFrameMemory(_currentFrameAddr);
            return true;
        }

        public unsafe Task SendBuffer(ReadOnlyMemory<byte> buffer)
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(AF_XDPTransmitRing));

            if (!_hasReservedSlot)
            {
                throw new InvalidOperationException("No TX ring slot reserved. Call TryGetNextBuffer first.");
            }

            // Get TX descriptor for the previously reserved slot
            nint descPtr = xsk_ring_prod__tx_desc(ref _txRing, _reservedIdx);
            xdp_desc* desc = (xdp_desc*)descPtr.ToPointer();

            // Set descriptor fields
            desc->addr = _currentFrameAddr;
            desc->len = (uint)buffer.Length;
            desc->options = 0;

            // Submit the packet
            xsk_ring_prod__submit(ref _txRing, 1);

            // Mark slot as no longer reserved
            _hasReservedSlot = false;

            // Wake up kernel if needed
            if (xsk_ring_prod__needs_wakeup(ref _txRing))
            {
                sendto(_socketFd, 0, 0, MSG_DONTWAIT, 0, 0);
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

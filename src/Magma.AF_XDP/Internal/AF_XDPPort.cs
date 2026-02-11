using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Magma.AF_XDP.Interop;
using Magma.Network.Abstractions;
using static Magma.AF_XDP.Interop.LibBpf;

namespace Magma.AF_XDP.Internal
{
    /// <summary>
    /// AF_XDP port managing XDP socket lifecycle and packet rings
    /// </summary>
    /// <typeparam name="TReceiver">Type of packet receiver</typeparam>
    public class AF_XDPPort<TReceiver> : IDisposable
        where TReceiver : IPacketReceiver
    {
        private readonly string _interfaceName;
        private readonly uint _queueId;
        private readonly AF_XDPTransportOptions _options;
        private readonly Func<AF_XDPTransmitRing, TReceiver> _receiverFactory;
        private nint _xskSocket;
        private AF_XDPMemoryManager _memoryManager;
        private AF_XDPTransmitRing _transmitRing;
        private TReceiver _receiver;
        private xsk_ring_cons _rxRing;
        private xsk_ring_prod _txRing;
        private Thread _receiveThread;
        private bool _disposed;
        private volatile bool _running;

        public AF_XDPPort(string interfaceName, Func<AF_XDPTransmitRing, TReceiver> receiverFactory, AF_XDPTransportOptions options = null)
        {
            _interfaceName = interfaceName ?? throw new ArgumentNullException(nameof(interfaceName));
            _receiverFactory = receiverFactory ?? throw new ArgumentNullException(nameof(receiverFactory));
            _options = options ?? new AF_XDPTransportOptions { InterfaceName = interfaceName };
            _queueId = (uint)_options.QueueId;
        }

        public void Open()
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(AF_XDPPort<TReceiver>));

            try
            {
                // Create UMEM (User Memory) area
                _memoryManager = new AF_XDPMemoryManager(
                    (uint)_options.UmemFrameCount,
                    (uint)_options.FrameSize);

                Console.WriteLine($"Created UMEM with {_options.UmemFrameCount} frames of {_options.FrameSize} bytes");

                // Configure XDP socket
                var socketConfig = new xsk_socket_config
                {
                    rx_size = (uint)_options.RxRingSize,
                    tx_size = (uint)_options.TxRingSize,
                    libbpf_flags = 0,
                    xdp_flags = 0, // XDP_FLAGS_UPDATE_IF_NOEXIST
                    bind_flags = (ushort)(_options.UseZeroCopy ? XskBindFlags.XDP_ZEROCOPY : XskBindFlags.XDP_COPY)
                };

                // Create XDP socket
                int ret = xsk_socket__create(
                    out _xskSocket,
                    _interfaceName,
                    _queueId,
                    _memoryManager.Umem,
                    ref _rxRing,
                    ref _txRing,
                    ref socketConfig);

                if (ret != 0)
                {
                    throw new InvalidOperationException($"Failed to create XDP socket on {_interfaceName}: error code {ret}");
                }

                Console.WriteLine($"AF_XDP socket created on interface {_interfaceName}, queue {_queueId}");

                // Create transmit ring
                _transmitRing = new AF_XDPTransmitRing(_xskSocket, _memoryManager, ref _txRing, _interfaceName);
                _receiver = _receiverFactory(_transmitRing);

                Console.WriteLine($"AF_XDP port opened on interface {_interfaceName}");

                // Start packet receive loop
                _running = true;
                _receiveThread = new Thread(ReceiveLoop)
                {
                    IsBackground = true,
                    Name = $"AF_XDP RX {_interfaceName}"
                };
                _receiveThread.Start();

                Console.WriteLine("AF_XDP receive thread started");
            }
            catch
            {
                // Cleanup on error
                Dispose();
                throw;
            }
        }

        private unsafe void ReceiveLoop()
        {
            Console.WriteLine($"Starting receive loop for {_interfaceName}");

            while (_running)
            {
                try
                {
                    // Check for received packets
                    uint idx;
                    uint rcvd = xsk_ring_cons__peek(ref _rxRing, 64, out idx);

                    if (rcvd > 0)
                    {
                        // Process received packets
                        // NOTE: This is a simplified implementation for demonstration
                        // Production use requires integrating with IPacketReceiver
                        for (uint i = 0; i < rcvd; i++)
                        {
                            nint descPtr = xsk_ring_cons__rx_desc(ref _rxRing, idx + i);
                            xdp_desc* desc = (xdp_desc*)descPtr.ToPointer();

                            // Get packet data from UMEM
                            Memory<byte> packet = _memoryManager.GetFrameMemory(desc->addr).Slice(0, (int)desc->len);

                            // TODO: Pass to receiver with proper memory ownership
                            // _receiver.TryConsume(ownedMemory) would handle the packet
                        }

                        // Release RX descriptors
                        xsk_ring_cons__release(ref _rxRing, rcvd);

                        // Replenish fill ring
                        ReplenishFillRing(rcvd);
                    }
                    else
                    {
                        // No packets, sleep briefly to avoid busy-wait
                        Thread.Sleep(1);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error in receive loop: {ex.Message}");
                    Thread.Sleep(100);
                }
            }

            Console.WriteLine($"Receive loop for {_interfaceName} stopped");
        }

        private unsafe void ReplenishFillRing(uint count)
        {
            // Replenish fill ring with completed frames
            uint idx;
            uint reserved = xsk_ring_prod__reserve(ref _memoryManager.FillRing, count, out idx);

            for (uint i = 0; i < reserved; i++)
            {
                // Get completed frame address from completion ring
                uint compIdx;
                if (xsk_ring_cons__peek(ref _memoryManager.CompRing, 1, out compIdx) > 0)
                {
                    nint compAddrPtr = xsk_ring_cons__comp_addr(ref _memoryManager.CompRing, compIdx);
                    ulong frameAddr = *(ulong*)compAddrPtr.ToPointer();
                    xsk_ring_cons__release(ref _memoryManager.CompRing, 1);

                    // Add frame back to fill ring
                    nint fillAddrPtr = xsk_ring_prod__fill_addr(ref _memoryManager.FillRing, idx + i);
                    *(ulong*)fillAddrPtr.ToPointer() = frameAddr;
                }
                else
                {
                    // If no completed frames, use next available frame
                    nint fillAddrPtr = xsk_ring_prod__fill_addr(ref _memoryManager.FillRing, idx + i);
                    *(ulong*)fillAddrPtr.ToPointer() = (idx + i) * _memoryManager.FrameSize;
                }
            }

            if (reserved > 0)
            {
                xsk_ring_prod__submit(ref _memoryManager.FillRing, reserved);
            }
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                // Stop receive thread
                _running = false;
                _receiveThread?.Join(TimeSpan.FromSeconds(5));

                // Cleanup transmit ring
                _transmitRing?.Dispose();

                // Close XDP socket
                if (_xskSocket != 0)
                {
                    xsk_socket__delete(_xskSocket);
                    _xskSocket = 0;
                    Console.WriteLine("XDP socket deleted");
                }

                // Cleanup UMEM
                _memoryManager?.Dispose();

                _disposed = true;
            }
        }
    }
}

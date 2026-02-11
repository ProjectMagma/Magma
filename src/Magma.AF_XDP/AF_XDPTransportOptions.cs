using System;

namespace Magma.AF_XDP
{
    /// <summary>
    /// Configuration options for AF_XDP transport
    /// </summary>
    public class AF_XDPTransportOptions
    {
        /// <summary>
        /// Network interface name (e.g., "eth0")
        /// </summary>
        public string InterfaceName { get; set; }

        /// <summary>
        /// Queue ID to bind to (default: 0)
        /// </summary>
        public int QueueId { get; set; } = 0;

        /// <summary>
        /// Use zero-copy mode if supported by NIC driver
        /// </summary>
        public bool UseZeroCopy { get; set; } = true;

        /// <summary>
        /// Number of frames in UMEM (User Memory) area
        /// Must be power of 2
        /// </summary>
        public int UmemFrameCount { get; set; } = 4096;

        /// <summary>
        /// Size of each frame in bytes
        /// </summary>
        public int FrameSize { get; set; } = 2048;

        /// <summary>
        /// Number of descriptors in RX ring
        /// Must be power of 2
        /// </summary>
        public int RxRingSize { get; set; } = 2048;

        /// <summary>
        /// Number of descriptors in TX ring
        /// Must be power of 2
        /// </summary>
        public int TxRingSize { get; set; } = 2048;
    }
}

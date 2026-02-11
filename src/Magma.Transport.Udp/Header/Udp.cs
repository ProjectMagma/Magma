using System;
using System.Net;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Magma.Network.Header
{
    /// <summary>
    /// Represents a UDP header as defined in RFC 768.
    /// UDP header is 8 bytes: Source Port (2), Destination Port (2), Length (2), Checksum (2)
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Udp
    {
        private ushort _sourcePort;
        private ushort _destinationPort;
        private ushort _length;
        private ushort _checksum;

        /// <summary>
        /// Source port number.
        /// </summary>
        public ushort SourcePort
        {
            get => (ushort)IPAddress.NetworkToHostOrder((short)_sourcePort);
            set => _sourcePort = (ushort)IPAddress.HostToNetworkOrder((short)value);
        }

        /// <summary>
        /// Destination port number.
        /// </summary>
        public ushort DestinationPort
        {
            get => (ushort)IPAddress.NetworkToHostOrder((short)_destinationPort);
            set => _destinationPort = (ushort)IPAddress.HostToNetworkOrder((short)value);
        }

        /// <summary>
        /// Length in bytes of the UDP header and data.
        /// Minimum value is 8 (header only).
        /// </summary>
        public ushort Length
        {
            get => (ushort)IPAddress.NetworkToHostOrder((short)_length);
            set => _length = (ushort)IPAddress.HostToNetworkOrder((short)value);
        }

        /// <summary>
        /// Checksum for error-checking of the header and data.
        /// </summary>
        public ushort Checksum
        {
            get => (ushort)IPAddress.NetworkToHostOrder((short)_checksum);
            set => _checksum = (ushort)IPAddress.HostToNetworkOrder((short)value);
        }

        /// <summary>
        /// Attempts to parse a UDP header from the input span.
        /// </summary>
        /// <param name="input">Input byte span containing UDP packet</param>
        /// <param name="udp">Parsed UDP header</param>
        /// <param name="data">Remaining data after the UDP header</param>
        /// <returns>True if parsing succeeded, false otherwise</returns>
        public static bool TryConsume(ReadOnlySpan<byte> input, out Udp udp, out ReadOnlySpan<byte> data)
        {
            if (input.Length >= Unsafe.SizeOf<Udp>())
            {
                udp = Unsafe.As<byte, Udp>(ref MemoryMarshal.GetReference(input));
                data = input.Slice(Unsafe.SizeOf<Udp>());
                return true;
            }

            udp = default;
            data = default;
            return false;
        }

        public override string ToString() =>
            "+- UDP Datagram -----------------------------------------------------------------------+" + Environment.NewLine +
            $"| :{SourcePort.ToString()} -> :{DestinationPort.ToString()} Length: {Length} Checksum: 0x{Checksum:X4}".PadRight(87) + "|";
    }
}

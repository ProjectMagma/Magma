
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Magma.Internet.Ip;

using static Magma.Network.IPAddress;

namespace Magma.Network.Header
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct IPv4
    {
        // No IPv4 Options data in this struct
        private byte _versionAndHeaderLength;
        private byte _dscpAndEcn;
        private ushort _totalLength;
        private ushort _identification;
        private ushort _flagsAndFragmentOffset;
        private byte _ttl;
        private ProtocolNumber _protocol;
        private ushort _checksum;
        private V4Address _sourceIPAdress;
        private V4Address _destinationIPAdress;

        /// <summary>
        /// The first header field in an IP packet is the four-bit version field. For IPv4, this is always equal to 4.
        /// </summary>
        public byte Version => (byte)(_versionAndHeaderLength & 0b_0000_1111);

        /// <summary>
        /// The Internet Header Length (IHL) field has 4 bits, which is the number of 32-bit words. 
        /// </summary>
        /// <remarks>
        /// Since an IPv4 header may contain a variable number of options, this field specifies the size of the header (this also coincides with the offset to the data). 
        /// The minimum value for this field is 5, which indicates a length of 5 × 32 bits = 160 bits = 20 bytes. 
        /// As a 4-bit field, the maximum value is 15 words (15 × 32 bits, or 480 bits = 60 bytes).
        /// </remarks>
        public byte InternetHeaderLength => (byte)(_versionAndHeaderLength >> 4);

        /// <summary>
        /// This field is defined by RFC 2474 (updated by RFC 3168 and RFC 3260) for Differentiated services (DiffServ). 
        /// New technologies are emerging that require real-time data streaming and therefore make use of the DSCP field. 
        /// An example is Voice over IP (VoIP), which is used for interactive data voice exchange.
        /// </summary>
        public byte DifferentiatedServicesCodePoint => (byte)(_versionAndHeaderLength & 0x3f);

        /// <summary>
        /// This field is defined in RFC 3168 and allows end-to-end notification of network congestion without dropping packets. 
        /// ECN is an optional feature that is only used when both endpoints support it and are willing to use it.
        /// It is only effective when supported by the underlying network.
        /// </summary>
        public byte ExplicitCongestionNotification => (byte)(_versionAndHeaderLength >> 6);

        /// <summary>
        /// This 16-bit field defines the entire packet size in bytes, including header and data.
        /// The minimum size is 20 bytes (header without data) and the maximum is 65,535 bytes. 
        /// All hosts are required to be able to reassemble datagrams of size up to 576 bytes, but most modern hosts handle much larger packets. 
        /// </summary>
        /// <remarks>
        /// Sometimes links impose further restrictions on the packet size, in which case datagrams must be fragmented.
        /// Fragmentation in IPv4 is handled in either the host or in routers.
        /// </remarks>
        public ushort TotalLength => (ushort)System.Net.IPAddress.NetworkToHostOrder((short)_totalLength);

        public ushort HeaderLength => (ushort)((_versionAndHeaderLength & 0xf) * 4);
        public ushort DataLength => (ushort)(TotalLength - HeaderLength);

        /// <summary>
        /// This field is an identification field and is primarily used for uniquely identifying the group of fragments of a single IP datagram. 
        /// </summary>
        /// <remarks>
        /// Some experimental work has suggested using the ID field for other purposes, 
        /// such as for adding packet-tracing information to help trace datagrams with spoofed source addresses, 
        /// but RFC 6864 now prohibits any such use.
        /// </remarks>
        public ushort Identification => _identification;

        /// <summary>
        /// A three-bit field follows and is used to control or identify fragments. They are (in order, from most significant to least significant):
        /// bit 0: Reserved; must be zero.
        /// bit 1: Don't Fragment (DF)
        /// bit 2: More Fragments(MF)
        /// </summary>
        /// <remarks>
        /// If the DF flag is set, and fragmentation is required to route the packet, then the packet is dropped. 
        /// This can be used when sending packets to a host that does not have sufficient resources to handle fragmentation. 
        /// It can also be used for Path MTU Discovery, either automatically by the host IP software, 
        /// or manually using diagnostic tools such as ping or traceroute. 
        /// For unfragmented packets, the MF flag is cleared. For fragmented packets, all fragments except the last have the MF flag set. 
        /// The last fragment has a non-zero Fragment Offset field, differentiating it from an unfragmented packet.
        /// </remarks>
        public byte Flags => (byte)(_flagsAndFragmentOffset >> 13);

        /// <summary>
        /// The fragment offset field is measured in units of eight-byte blocks.
        /// </summary>
        /// <remarks>
        /// It is 13 bits long and specifies the offset of a particular fragment relative to the beginning of the original unfragmented IP datagram. 
        /// The first fragment has an offset of zero. 
        /// This allows a maximum offset of (213 – 1) × 8 = 65,528 bytes, 
        /// which would exceed the maximum IP packet length of 65,535 bytes with the header length included (65,528 + 20 = 65,548 bytes).
        /// </remarks>
        public ushort FragmentOffset => (ushort)(_flagsAndFragmentOffset & 0b_0001_1111_1111_1111);

        /// <summary>
        /// An eight-bit time to live field helps prevent datagrams from persisting n an internet. 
        /// This field limits a datagram's lifetime and is a hop count - 
        /// when the datagram arrives at a router, the router decrements the TTL field by one. 
        /// </summary>
        public byte TimeToLive => _ttl;

        /// <summary>
        /// This field defines the protocol used in the data portion of the IP datagram.
        /// </summary>
        /// <remarks>
        /// The Internet Assigned Numbers Authority maintains a list of IP protocol numbers which was originally defined in RFC 790
        /// </remarks>
        public ProtocolNumber Protocol { get => _protocol; set => _protocol = value; }

        /// <summary>
        /// The 16-bit checksum field is used for error-checking of the header.
        /// When a packet arrives at a router, the router calculates the checksum of the header and compares it to the checksum field.
        /// If the values do not match, the router discards the packet. 
        /// </summary>
        /// <remarks>
        /// When a packet arrives at a router, the router decreases the TTL field. Consequently, the router must calculate a new checksum.
        /// </remarks>
        public ushort HeaderChecksum
        {
            get => _checksum;
            set => _checksum = value;
        }

        /// <summary>
        /// This field is the IPv4 address of the sender of the packet. 
        /// Note that this address may be changed in transit by a network address translation device.
        /// </summary>
        public V4Address SourceAddress
        {
            get => _sourceIPAdress;
            set => _sourceIPAdress = value;
        }

        /// <summary>
        /// This field is the IPv4 address of the receiver of the packet.
        /// As with the source address, this may be changed in transit by a network address translation device.
        /// </summary>
        public V4Address DestinationAddress
        {
            get => _destinationIPAdress;
            set => _destinationIPAdress = value;
        }

        public static bool TryConsume(ReadOnlySpan<byte> input, out IPv4 ip, out ReadOnlySpan<byte> data, bool doChecksum = true)
        {
            if (input.Length >= Unsafe.SizeOf<IPv4>())
            {
                ip = Unsafe.As<byte, IPv4>(ref MemoryMarshal.GetReference(input));
                var totalSize = ip.TotalLength;
                var headerSize = ip.HeaderLength;
                if ((uint)totalSize >= (uint)headerSize && (uint)totalSize == (uint)input.Length && (doChecksum || ip.IsChecksumValid()))
                {
                    data = input.Slice(headerSize);
                    return true;
                }
            }

            ip = default;
            data = default;
            return false;
        }

        public unsafe override string ToString()
        {
            return "+- IPv4 Datagram ----------------------------------------------------------------------+" + Environment.NewLine +
                  $"| {Protocol.ToString().PadRight(11)} | {SourceAddress.ToString()} -> {DestinationAddress.ToString()} | Length: {TotalLength}, H: {HeaderLength}, D: {DataLength}".PadRight(86) +
                  (IsChecksumValid() ? " " : "X")
                  + "|";
        }

        public bool IsChecksumValid() => Checksum.IsValid(ref Unsafe.As<IPv4, byte>(ref this), Unsafe.SizeOf<IPv4>());
    }
}

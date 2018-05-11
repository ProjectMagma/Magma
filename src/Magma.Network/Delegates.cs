using System;
using Magma.Network.Header;

namespace Magma.Network
{
    public delegate bool ConsumeInternetLayer(in Ethernet ethernetFrame, ReadOnlySpan<byte> input);
    public delegate bool IPv4ConsumeTransportLayer(in Ethernet ethernetFrame, in IPv4 ipv4, ReadOnlySpan<byte> input);
    public delegate bool IPv6ConsumeTransportLayer(in Ethernet ethernetFrame, in IPv6 ipv6, ReadOnlySpan<byte> input);
}

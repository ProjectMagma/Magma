using System;
using Magma.Network.Abstractions;
using Magma.Network.Header;

namespace Magma.Network
{
    public struct DefaultPacketReceiver : IPacketReceiver
    {
        private PacketReceiver _packetReceiver;

        public static DefaultPacketReceiver CreateDefault() => 
            new DefaultPacketReceiver()
            {
                _packetReceiver = PacketReceiver.CreateDefault()
            };

        public bool TryConsume(ReadOnlySpan<byte> input) => _packetReceiver.TryConsume(input);
    }

    internal class PacketReceiver : IPacketReceiver
    {
        private readonly static ConsumeInternetLayer _ipv4Consumer = (in Ethernet ethernetFrame, ReadOnlySpan<byte> input)
            => TryConsumeIPv4(in ethernetFrame, input);
        private readonly static ConsumeInternetLayer _ipv6Consumer = (in Ethernet ethernetFrame, ReadOnlySpan<byte> input)
            => TryConsumeIPv6(in ethernetFrame, input);
        private readonly static ConsumeInternetLayer _arpConsumer = (in Ethernet ethernetFrame, ReadOnlySpan<byte> input)
            => TryConsumeArp(in ethernetFrame, input);

        public ConsumeInternetLayer IPv4Consumer { get; set; }
        public ConsumeInternetLayer IPv6Consumer { get; set; }
        public ConsumeInternetLayer ArpConsumer { get; set; }

        public PacketReceiver() { }

        public static PacketReceiver CreateDefault() => new PacketReceiver()
        {
            IPv4Consumer = _ipv4Consumer,
            IPv6Consumer = _ipv6Consumer,
            ArpConsumer = _arpConsumer
        };

        public bool TryConsume(ReadOnlySpan<byte> input)
        {
            if (Ethernet.TryConsume(input, out var ethernetFrame, out var data))
            {
                switch (ethernetFrame.Ethertype)
                {
                    case EtherType.IPv4:
                        return IPv4Consumer?.Invoke(in ethernetFrame, data) ?? false;
                    case EtherType.IPv6:
                        return IPv6Consumer?.Invoke(in ethernetFrame, data) ?? false;
                    case EtherType.Arp:
                        return ArpConsumer?.Invoke(in ethernetFrame, data) ?? false;
                    default:
                        // Unsupported protocol pass to host OS networking
                        return false;
                }
            }
            else
            {
                // Couldn't parse, consume the invalid packet
                return true;
            }
        }

        private static bool TryConsumeIPv4(in Ethernet ethernetFrame, ReadOnlySpan<byte> input)
        {
            return false;
        }

        private static bool TryConsumeIPv6(in Ethernet ethernetFrame, ReadOnlySpan<byte> input)
        {
            return false;
        }

        private static bool TryConsumeArp(in Ethernet ethernetFrame, ReadOnlySpan<byte> input)
        {
            return false;
        }
    }
}

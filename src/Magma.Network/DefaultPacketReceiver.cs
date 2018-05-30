using System;
using System.Buffers;
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

        public void FlushPendingAcks()
        {
            throw new NotImplementedException();
        }

        public T TryConsume<T>(T input) where T : IMemoryOwner<byte> => _packetReceiver.TryConsume(input);
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

        public T TryConsume<T>(T owner) where T : IMemoryOwner<byte>
        {
            var input = owner.Memory.Span;
            var result = false;
            if (Ethernet.TryConsume(input, out var ethernetFrame, out var data))
            {
                switch (ethernetFrame.Ethertype)
                {
                    case EtherType.IPv4:
                        result = IPv4Consumer?.Invoke(in ethernetFrame, data) ?? false;
                        break;
                    case EtherType.IPv6:
                        result = IPv6Consumer?.Invoke(in ethernetFrame, data) ?? false;
                        break;
                    case EtherType.Arp:
                        result = ArpConsumer?.Invoke(in ethernetFrame, data) ?? false;
                        break;
                    default:
                        // Unsupported protocol pass to host OS networking
                        result = false;
                        break;
                }
            }
            else
            {
                // Couldn't parse, consume the invalid packet
                result = true;
            }
            if(result)
            {
                owner.Dispose();
                return default;
            }
            return owner;
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

        public void FlushPendingAcks()
        {
            throw new NotImplementedException();
        }
    }
}

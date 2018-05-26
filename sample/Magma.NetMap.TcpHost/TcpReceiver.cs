using System;
using System.Buffers;
using System.Collections.Generic;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using Magma.Network.Abstractions;
using Magma.Network.Header;
using Microsoft.AspNetCore.Server.Kestrel.Transport.Abstractions.Internal;
using static Magma.Network.IPAddress;

namespace Magma.NetMap.TcpHost
{
    public class TcpReceiver : IPacketReceiver
    {
        private NetMapTransmitRing _transmitRing;
        private IConnectionDispatcher _connectionDispatcher;
        private IPEndPoint _ipEndPoint;
        private V4Address _address;
        private bool _isAny;
        private ushort _port;
        private Dictionary<(V4Address address, ushort port), NetmapTcpConnection> _connections = new Dictionary<(V4Address address, ushort port), NetmapTcpConnection>();
        private Random _randomSequenceNumber = new Random();

        public TcpReceiver(IPEndPoint ipEndPoint, NetMapTransmitRing transmitRing, IConnectionDispatcher connectionDispatcher)
        {
            _ipEndPoint = ipEndPoint;
            _transmitRing = transmitRing;
            _connectionDispatcher = connectionDispatcher;
            var bytes = _ipEndPoint.Address.GetAddressBytes();
            _address = Unsafe.As<byte, V4Address>(ref bytes[0]);
            _port = (ushort)_ipEndPoint.Port;
            _isAny = _ipEndPoint.Address == IPAddress.Any;
        }

        public NetMapTransmitRing Transmitter => _transmitRing;

        public uint RandomSeqeunceNumber() => (uint)_randomSequenceNumber.Next();

        public T TryConsume<T>(T input) where T : IMemoryOwner<byte>
        {
            var span = input.Memory.Span;

            if (!Ethernet.TryConsume(span, out var etherHeader, out var data)) return input;

            if (!IPv4.TryConsume(data, out var ipHeader, out data, false)) return input;

            // Now we will check the IP Checksum because we actually care
            if (ipHeader.Protocol != Internet.Ip.ProtocolNumber.Tcp
                || (!_isAny && ipHeader.DestinationAddress != _address)
                || !ipHeader.IsChecksumValid()) return input;

            // So we have TCP lets parse out the header
            if (!Tcp.TryConsume(data, out var tcp, out data)
                || tcp.DestinationPort != _port) return input;

            // okay we now have some data we care about all the rest has been ditched to the host rings
            if (!_connections.TryGetValue((ipHeader.SourceAddress, tcp.SourcePort), out var connection))
            {
                // We need to create a connection because we don't know this one but only if the packet is a syn
                // otherwise this is garbage and we should just swallow it
                if (!tcp.SYN)
                {
                    input.Dispose();
                    return default;
                }

                // So looks like we need to create a connection then
                connection = new NetmapTcpConnection(ipHeader.SourceAddress, ipHeader.DestinationAddress, etherHeader.Source, etherHeader.Destination, this);
                _connections[(ipHeader.SourceAddress, tcp.SourcePort)] = connection;
                _connectionDispatcher.OnConnection(connection.Connection);
            }

            connection.ProcessPacket(tcp, data);
            return input;
        }
    }
}

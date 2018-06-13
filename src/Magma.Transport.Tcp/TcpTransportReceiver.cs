using System;
using System.Collections.Generic;
using System.Text;
using Magma.Network.Abstractions;
using Microsoft.AspNetCore.Server.Kestrel.Transport.Abstractions.Internal;
using static Magma.Network.IPAddress;
using System.Net;
using System.Runtime.CompilerServices;
using System.Buffers;
using Magma.Transport.Tcp.Header;
using Magma.Network.Header;

namespace Magma.Transport.Tcp
{
    public class TcpTransportReceiver<TTransmitter> : IPacketReceiver where TTransmitter : IPacketTransmitter
    {
        private TTransmitter _transmitRing;
        private IConnectionDispatcher _connectionDispatcher;
        private IPEndPoint _ipEndPoint;
        private V4Address _address;
        private bool _isAny;
        private ushort _port;
        private Dictionary<(V4Address address, ushort port), TcpConnection<TTransmitter>> _connections = new Dictionary<(V4Address address, ushort port), TcpConnection<TTransmitter>>();
        private Random _randomSequenceNumber = new Random();
        private TcpConnection<TTransmitter> _lastConnectionSentTo;

        public TcpTransportReceiver(IPEndPoint ipEndPoint, TTransmitter transmitRing, IConnectionDispatcher connectionDispatcher)
        {
            _ipEndPoint = ipEndPoint;
            _transmitRing = transmitRing;
            _connectionDispatcher = connectionDispatcher;
            var bytes = _ipEndPoint.Address.GetAddressBytes();
            _address = Unsafe.As<byte, V4Address>(ref bytes[0]);
            _port = (ushort)_ipEndPoint.Port;
            _isAny = _ipEndPoint.Address == IPAddress.Any;
        }

        public TTransmitter Transmitter => _transmitRing;

        public void FlushPendingAcks() => _lastConnectionSentTo?.SendAckIfRequired();

        public uint RandomSeqeunceNumber() => (uint)_randomSequenceNumber.Next();

        public T TryConsume<T>(T input) where T : IMemoryOwner<byte>
        {
            try
            {
                var span = input.Memory.Span;
                if (!Ethernet.TryConsume(span, out var etherHeader, out var data)) return input;
                if (!IPv4.TryConsume(data, out var ipHeader, out data, false)) return input;

                // Now we will check the IP Checksum because we actually care
                if (ipHeader.Protocol != Internet.Ip.ProtocolNumber.Tcp
                    || (!_isAny && ipHeader.DestinationAddress != _address)
                    || !ipHeader.IsChecksumValid()) return input;

                // So we have TCP lets parse out the header
                if (!TcpHeaderWithOptions.TryConsume(data, out var tcp, out data)
                    || tcp.Header.DestinationPort != _port) return input;
                try
                {
                    // okay we now have some data we care about all the rest has been ditched to the host rings
                    if (!_connections.TryGetValue((ipHeader.SourceAddress, tcp.Header.SourcePort), out var connection))
                    {
                        _lastConnectionSentTo?.SendAckIfRequired();
                        // We need to create a connection because we don't know this one but only if the packet is a syn
                        // otherwise this is garbage and we should just swallow it
                        if (!tcp.Header.SYN)
                        {
                            return default;
                        }

                        // So looks like we need to create a connection then
                        connection = new TcpConnection<TTransmitter>(etherHeader, ipHeader, tcp.Header, Transmitter, _connectionDispatcher);
                        _connections[(ipHeader.SourceAddress, tcp.Header.SourcePort)] = connection;
                    }
                    else if (connection != _lastConnectionSentTo)
                    {
                        _lastConnectionSentTo.SendAckIfRequired();
                    }

                    connection.ProcessPacket(tcp, data);
                    _lastConnectionSentTo = connection;
                    return default;
                }
                finally
                {
                    input.Dispose();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing packet!!! thread will die exception was {ex}");
                return input;
            }
        }
    }
}

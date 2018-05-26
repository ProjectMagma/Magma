using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using Magma.Link;
using Magma.Network.Header;
using Microsoft.AspNetCore.Server.Kestrel.Transport.Abstractions.Internal;
using static Magma.Network.IPAddress;

namespace Magma.NetMap.TcpHost
{
    public class NetmapTcpConnection
    {
        private TcpReceiver _tcpReceiver;
        private TransportConnection _connection;
        private TcpConnectionState _state = TcpConnectionState.Listen;
        private uint _receiveSequenceNumber;
        private uint _sendSequenceNumber;
        private ushort _remotePort;
        private ushort _localPort;
        private V4Address _remoteAddress;
        private V4Address _localAddress;
        private MacAddress _remoteMac;
        private MacAddress _localMac;

        public NetmapTcpConnection(V4Address remoteAddress, V4Address localAddress, MacAddress remoteMac, MacAddress localMac, TcpReceiver tcpReceiver)
        {
            _remoteAddress = remoteAddress;
            _localAddress = localAddress;
            _remoteMac = remoteMac;
            _localMac = localMac;
            _tcpReceiver = tcpReceiver;
            _connection = new TransportConnection();
        }

        public TransportConnection Connection => _connection;

        public void ProcessPacket(Tcp header, ReadOnlySpan<byte> data)
        {
            switch(_state)
            {
                case TcpConnectionState.Listen:
                    // We know we checked for syn in the upper layer so we can ignore that for now
                    _receiveSequenceNumber = header.SequenceNumber;
                    _sendSequenceNumber = _tcpReceiver.RandomSeqeunceNumber();
                    if(!WriteEthernetPacket(0, out var tcpHeader, out var dataSpan, out var memory))
                    {
                        throw new NotImplementedException("Need to handle this we don't have anyway to ack cause we have back pressure");
                    }
                    tcpHeader.ACK = true;
                    tcpHeader.AcknowledgmentNumber = _receiveSequenceNumber+1;
                    tcpHeader.SequenceNumber = _sendSequenceNumber;
                    tcpHeader.SYN = true;
                    _tcpReceiver.Transmitter.SendBuffer(memory);
                    _state = TcpConnectionState.Syn_Rcvd;
                    break;
                default:
                    throw new NotImplementedException($"Unknown tcp state?? {_state}");
            }
        }

        private bool WriteEthernetPacket(int dataSize, out Tcp tcpHeader, out Span<byte> dataSpan, out Memory<byte> memory)
        {
            if(!_tcpReceiver.Transmitter.TryGetNextBuffer(out memory))
            {
                dataSpan = default;
                tcpHeader = default;
                return false;
            }

            // We have the memory calculate the total size we need
            var totalSize = dataSize + Unsafe.SizeOf<Ethernet>() + Unsafe.SizeOf<IPv4>() + Unsafe.SizeOf<Tcp>();
            var span = memory.Span.Slice(0, totalSize);
            ref var pointer = ref MemoryMarshal.GetReference(span);

            var ethHeader = Unsafe.As<byte, Ethernet>(ref pointer);
            ethHeader.Destination = _remoteMac;
            ethHeader.Ethertype = Network.EtherType.IPv4;
            ethHeader.Source = _localMac;
            pointer = ref Unsafe.Add(ref pointer, Unsafe.SizeOf<Ethernet>());

            var ipHeader = Unsafe.As<byte, IPv4>(ref pointer);
            ipHeader.DestinationAddress = _remoteAddress;
            ipHeader.SourceAddress = _localAddress;
            ipHeader.Protocol = Internet.Ip.ProtocolNumber.Tcp;
            // -----> Help?? ipHeader.DataLength = totalSize - Unsafe.SizeOf<Ethernet>() - Unsafe.SizeOf<IPv4>();
            // What else needs to be set?
            throw new NotImplementedException();
            pointer = ref Unsafe.Add(ref pointer, Unsafe.SizeOf<IPv4>());

            tcpHeader = Unsafe.As<byte, Tcp>(ref pointer);
            tcpHeader.DestinationPort = _remotePort;
            tcpHeader.SourcePort = _localPort;
            memory = memory.Slice(0, totalSize);
            dataSpan = span.Slice(Unsafe.SizeOf<Ethernet>() + Unsafe.SizeOf<IPv4>() + Unsafe.SizeOf<Tcp>());
            return true;

        }
    }
}

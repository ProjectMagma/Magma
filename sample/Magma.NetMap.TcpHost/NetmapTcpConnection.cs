using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using Magma.Link;
using Magma.Network.Header;
using Microsoft.AspNetCore.Server.Kestrel.Transport.Abstractions.Internal;
using static Magma.Network.IPAddress;

namespace Magma.NetMap.TcpHost
{
    public class NetMapTcpConnection
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

        public NetMapTcpConnection(Ethernet ethHeader, IPv4 ipHeader,TcpReceiver tcpReceiver)
        {
            _remoteAddress = ipHeader.SourceAddress;
            _localAddress = ipHeader.DestinationAddress;
            _remoteMac = ethHeader.Source;
            _localMac = ethHeader.Destination;
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
                    _remotePort = header.SourcePort;
                    _localPort = header.DestinationPort;

                    Tcp tcpHeader = default;

                    if(!WriteEthernetPacket(0, ref tcpHeader, out var dataSpan, out var memory))
                    {
                        throw new NotImplementedException("Need to handle this we don't have anyway to ack cause we have back pressure");
                    }
                    tcpHeader.ACK = true;
                    tcpHeader.AcknowledgmentNumber = _receiveSequenceNumber+1;
                    tcpHeader.SequenceNumber = _sendSequenceNumber;
                    tcpHeader.SYN = true;
                    _tcpReceiver.Transmitter.SendBuffer(memory);
                    _tcpReceiver.Transmitter.ForceFlush();

                    _state = TcpConnectionState.Syn_Rcvd;
                    break;
                default:
                    Thread.Sleep(1000);
                    throw new NotImplementedException($"Unknown tcp state?? {_state}");
            }
        }

        private bool WriteEthernetPacket(int dataSize, ref Tcp tcpHeader, out Span<byte> dataSpan, out Memory<byte> memory)
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

            ref var ethHeader = ref Unsafe.As<byte, Ethernet>(ref pointer);
            ethHeader.Destination = _remoteMac;
            ethHeader.Ethertype = Network.EtherType.IPv4;
            ethHeader.Source = _localMac;
            pointer = ref Unsafe.Add(ref pointer, Unsafe.SizeOf<Ethernet>());

            ref var ipHeader = ref Unsafe.As<byte, IPv4>(ref pointer);
            IPv4.InitHeader(ref ipHeader, _localAddress, _remoteAddress, (ushort)(Unsafe.SizeOf<Tcp>() + dataSize), Internet.Ip.ProtocolNumber.Tcp, 41503);
            pointer = ref Unsafe.Add(ref pointer, Unsafe.SizeOf<IPv4>());

            // IP V4 done time to do the TCP packet;

            tcpHeader = ref Unsafe.As<byte, Tcp>(ref pointer);
            tcpHeader.DestinationPort = _remotePort;
            tcpHeader.SourcePort = _localPort;
            memory = memory.Slice(0, totalSize);
            dataSpan = span.Slice(Unsafe.SizeOf<Ethernet>() + Unsafe.SizeOf<IPv4>() + Unsafe.SizeOf<Tcp>());
            return true;

        }
    }
}

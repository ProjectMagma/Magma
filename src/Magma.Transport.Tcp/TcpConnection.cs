using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using Magma.Link;
using Magma.Network;
using Magma.Network.Header;
using Magma.Transport.Tcp.Header;
using static Magma.Network.IPAddress;

namespace Magma.Transport.Tcp
{
    public abstract class TcpConnection
    {
        private TcpConnectionState _state = TcpConnectionState.Listen;
        private uint _receiveSequenceNumber;
        private uint _sendSequenceNumber;
        private ushort _remotePort;
        private ushort _localPort;
        private V4Address _remoteAddress;
        private V4Address _localAddress;
        private MacAddress _remoteMac;
        private MacAddress _localMac;
        private byte _windowScale;
        private ushort _windowSize;
        private ulong _pseudoPartialSum;
        private uint _echoTimestamp;

        public TcpConnection(Ethernet ethHeader, IPv4 ipHeader)
        {
            _remoteAddress = ipHeader.SourceAddress;
            _localAddress = ipHeader.DestinationAddress;
            _remoteMac = ethHeader.Source;
            _localMac = ethHeader.Destination;
            var pseudo = new TcpV4PseudoHeader() { Destination = _remoteAddress, Source = _localAddress, ProtocolNumber = Internet.Ip.ProtocolNumber.Tcp, Reserved = 0 };
            _pseudoPartialSum = Checksum.PartialCalculate(ref Unsafe.As<TcpV4PseudoHeader, byte>(ref pseudo), Unsafe.SizeOf<TcpV4PseudoHeader>());
        }

        public void ProcessPacket(TcpHeaderWithOptions header, ReadOnlySpan<byte> data)
        {
            switch (_state)
            {
                case TcpConnectionState.Listen:
                    // We know we checked for syn in the upper layer so we can ignore that for now
                    _receiveSequenceNumber = header.Header.SequenceNumber;
                    _sendSequenceNumber = GetRandomSequenceStart();
                    _remotePort = header.Header.SourcePort;
                    _localPort = header.Header.DestinationPort;
                    _windowScale = header.WindowScale == 0 ? (byte)0x01 : header.WindowScale;
                    _windowSize = header.Header.WindowSize;
                    Network.Header.Tcp tcpHeader = default;

                    if (!WriteEthernetPacket(0, ref tcpHeader, out var dataSpan, out var memory, out var tcpSpan))
                    {
                        throw new NotImplementedException("Need to handle this we don't have anyway to ack cause we have back pressure");
                    }
                    tcpHeader.AcknowledgmentNumber = ++_receiveSequenceNumber;
                    tcpHeader.SequenceNumber = _sendSequenceNumber++;
                    tcpHeader.SYN = true;
                    tcpHeader.SetChecksum(tcpSpan, _pseudoPartialSum);
                    WriteMemory(memory);
                    _state = TcpConnectionState.Syn_Rcvd;
                    break;
                case TcpConnectionState.Syn_Rcvd:
                    if (header.Header.SYN)
                    {
                        Console.WriteLine("Another Syn made");
                        return;
                    }
                    else
                    {
                        Console.WriteLine("NOT ANOTHER SYN HOOOORAH");
                    }
                    Console.WriteLine($"Got a syn received with sequence number {header.Header.SequenceNumber} is it correct? {header.Header.SequenceNumber == _receiveSequenceNumber}");
                    Console.WriteLine($"Also the ack was {header.Header.AcknowledgmentNumber} is it correct? {header.Header.AcknowledgmentNumber == _sendSequenceNumber}");
                    _state = TcpConnectionState.Established;
                    break;
                default:
                    throw new NotImplementedException($"Unknown tcp state?? {_state}");
            }
        }

        private bool WriteEthernetPacket(int dataSize, ref Network.Header.Tcp tcpHeader, out Span<byte> dataSpan, out Memory<byte> memory, out Span<byte> tcpSpan)
        {
            if (!TryGetMemory(out memory))
            {
                dataSpan = default;
                tcpHeader = default;
                tcpSpan = default;
                return false;
            }

            // We have the memory calculate the total size we need
            var totalSize = dataSize + Unsafe.SizeOf<Ethernet>() + Unsafe.SizeOf<IPv4>() + Unsafe.SizeOf<Network.Header.Tcp>();
            var span = memory.Span.Slice(0, totalSize);
            ref var pointer = ref MemoryMarshal.GetReference(span);

            ref var ethHeader = ref Unsafe.As<byte, Ethernet>(ref pointer);
            ethHeader.Destination = _remoteMac;
            ethHeader.Ethertype = EtherType.IPv4;
            ethHeader.Source = _localMac;
            pointer = ref Unsafe.Add(ref pointer, Unsafe.SizeOf<Ethernet>());

            ref var ipHeader = ref Unsafe.As<byte, IPv4>(ref pointer);
            IPv4.InitHeader(ref ipHeader, _localAddress, _remoteAddress, (ushort)(Unsafe.SizeOf<Network.Header.Tcp>() + dataSize), Internet.Ip.ProtocolNumber.Tcp, 41503);
            pointer = ref Unsafe.Add(ref pointer, Unsafe.SizeOf<IPv4>());

            // IP V4 done time to do the TCP packet;

            tcpHeader = ref Unsafe.As<byte, Network.Header.Tcp>(ref pointer);
            tcpHeader.DestinationPort = _remotePort;
            tcpHeader.SourcePort = _localPort;
            tcpHeader.ACK = true;
            tcpHeader.Checksum = 0;
            tcpHeader.CWR = false;
            tcpHeader.DataOffset = 5;
            tcpHeader.ECE = false;
            tcpHeader.FIN = false;
            tcpHeader.NS = false;
            tcpHeader.PSH = true;
            tcpHeader.RST = false;
            tcpHeader.SYN = false;
            tcpHeader.URG = false;
            tcpHeader.UrgentPointer = 0;
            tcpHeader.WindowSize = _windowSize;
            memory = memory.Slice(0, totalSize);
            dataSpan = span.Slice(Unsafe.SizeOf<Ethernet>() + Unsafe.SizeOf<IPv4>() + Unsafe.SizeOf<Network.Header.Tcp>());
            tcpSpan = span.Slice(Unsafe.SizeOf<Ethernet>() + Unsafe.SizeOf<IPv4>());
            return true;
        }

        protected abstract void WriteMemory(Memory<byte> memory);
        protected abstract bool TryGetMemory(out Memory<byte> memory);
        protected abstract uint GetRandomSequenceStart();
        protected abstract uint GetTimestamp();
    }
}

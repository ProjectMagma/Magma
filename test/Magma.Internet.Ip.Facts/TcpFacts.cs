using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using Magma.Network;
using Magma.Network.Header;
using Magma.Transport.Tcp.Header;
using Xunit;
using static Magma.Network.IPAddress;

namespace Magma.Internet.Ip.Facts
{
    public class TcpFacts
    {
        private static readonly ushort _sourcePort = 49859;
        private static readonly ushort _destPort = 80;
        private static readonly byte[] _tcpSynPacketWithOptions = "c2 c3 00 50 d5 e2 df b4 00 00 00 00 a0 02 20 00 c4 47 00 00 02 04 05 b4 01 03 03 02 04 02 08 0a 00 04 aa 62 00 00 00 00".HexToByteArray();
                                                                //"C2-C3-00-50-D5-E2-DF-B4-00-00-00-00-A0-02-20-00-E9-85-00-00-02-04-05-B4-01-03-03-02-04-02-08-0A-00-04-AA-62-00-00-00-00"
        private static readonly byte[] _tcpSynPacket = _tcpSynPacketWithOptions.AsSpan().Slice(0, 20).ToArray();
        private static readonly uint _synSequenceNumber = 3588415412;
        private static readonly V4Address _sourceAddress = new V4Address(192, 168, 1, 104);
        private static readonly V4Address _destAddress = new V4Address(216,18,166,136);

        [Fact]
        public void CanReadTcpSyn()
        {
            var span = _tcpSynPacketWithOptions.AsSpan();

            var tcpHeader = Unsafe.As<byte, Tcp>(ref MemoryMarshal.GetReference(span));

            Assert.Equal(_sourcePort, tcpHeader.SourcePort);
            Assert.Equal(_destPort, tcpHeader.DestinationPort);
            Assert.Equal(0, tcpHeader.UrgentPointer);
            Assert.True(tcpHeader.SYN);
            Assert.False(tcpHeader.ACK);
            Assert.False(tcpHeader.CWR);
            Assert.False(tcpHeader.NS);
            Assert.False(tcpHeader.RST);
            Assert.False(tcpHeader.PSH);
            Assert.False(tcpHeader.URG);
            Assert.Equal(8192, tcpHeader.WindowSize);
            Assert.Equal(_synSequenceNumber, tcpHeader.SequenceNumber);
            Assert.Equal(0u, tcpHeader.AcknowledgmentNumber);
            Assert.Equal(10, tcpHeader.DataOffset);
        }

        [Fact]
        public void TryConsumeSynPacket()
        {
            var span = _tcpSynPacketWithOptions.AsSpan();

            Assert.True(Tcp.TryConsume(span, out var tcp, out var options, out var data));
            Assert.Equal(20, options.Length);
            Assert.Equal(0, data.Length);
        }

        [Fact]
        public void TryConsumeSynPacketWithOptions()
        {
            var span = _tcpSynPacketWithOptions.AsSpan();

            Assert.True(TcpHeaderWithOptions.TryConsume(span, out var header, out var data));
            Assert.True(header.SackPermitted);
            Assert.Equal(1460, header.MaximumSegmentSize);
            Assert.Equal(2, header.WindowScale);
        }

        [Fact]
        public void CanWriteTcpSyn()
        {
            var span = Enumerable.Repeat<byte>(0xFF, 10 * 4).ToArray().AsSpan();
            _tcpSynPacketWithOptions.AsSpan().Slice(20).CopyTo(span.Slice(20));
            ref var tcpHeader = ref Unsafe.As<byte, Tcp>(ref MemoryMarshal.GetReference(span));
            tcpHeader.AcknowledgmentNumber = 0;
            tcpHeader.Checksum = 0;
            tcpHeader.DestinationPort = _destPort;
            tcpHeader.SourcePort = _sourcePort;
            
            tcpHeader.UrgentPointer = 0;
            tcpHeader.SequenceNumber = _synSequenceNumber;
            tcpHeader.NS = false;
            tcpHeader.CWR = false;
            tcpHeader.ECE = false;
            tcpHeader.URG = false;
            tcpHeader.ACK = false;
            tcpHeader.PSH = false;
            tcpHeader.RST = false;
            tcpHeader.SYN = true;
            tcpHeader.FIN = false;
            tcpHeader.DataOffset = 10;
            tcpHeader.WindowSize = 8192;

            var pseudoHeader = new TcpV4PseudoHeader()
            {
                Destination = _destAddress,
                Source = _sourceAddress,
                ProtocolNumber = ProtocolNumber.Tcp,
                Reserved = 0,
                Size = (ushort)span.Length,
            };

            var checksumSpan = (new byte[span.Length + Unsafe.SizeOf<TcpV4PseudoHeader>()]).AsSpan();
            span.CopyTo(checksumSpan.Slice(Unsafe.SizeOf<TcpV4PseudoHeader>()));
            ref var pseudo = ref Unsafe.As<byte, TcpV4PseudoHeader>(ref MemoryMarshal.GetReference(checksumSpan));
            pseudo.Destination = _destAddress;
            pseudo.ProtocolNumber = ProtocolNumber.Tcp;
            pseudo.Reserved = 0;
            pseudo.Size = (ushort)span.Length;
            pseudo.Source = _sourceAddress;

            tcpHeader.Checksum = Checksum.Calculate(ref MemoryMarshal.GetReference(checksumSpan), checksumSpan.Length);
            //var temp = Checksum.PartialCalculate(ref Unsafe.As<TcpV4PseudoHeader, byte>(ref pseudoHeader), Unsafe.SizeOf<TcpV4PseudoHeader>());
            //tcpHeader.Checksum = Checksum.Calculate(ref MemoryMarshal.GetReference(span), 32, temp);

            for (var i = 0; i < _tcpSynPacket.Length;i++)
            {
                Assert.Equal(_tcpSynPacket[i], span[i]);
            }

            Assert.Equal(_tcpSynPacket, span.ToArray());
        }
    }
}

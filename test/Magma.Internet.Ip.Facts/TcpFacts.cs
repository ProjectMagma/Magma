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
        private static readonly ushort _sourcePort = 57598;
        private static readonly ushort _destPort = 6667;
        private static readonly byte[] _tcpSynPacketWithOptions = "e0 fe 1a 0b 7b 2b 85 36 00 00 00 00 80 02 fa f0 1b 94 00 00 02 04 05 b4 01 03 03 08 01 01 04 02".HexToByteArray();
                                                                //"E0-FE-1A-0B-7B-2B-85-36-00-00-00-00-80-02-FA-F0-78-DA-00-00-02-04-05-B4-01-03-03-08-01-01-04-02"
                                                                //Wireshark says the checksum is wrong on this and should be 0x520c so either will do me
        private static readonly byte[] _tcpSynPacket = _tcpSynPacketWithOptions.AsSpan().Slice(0, 20).ToArray();
        private static readonly uint _synSequenceNumber = 0x36852b7b;
        private static readonly V4Address _sourceAddress = new V4Address(172, 18, 225, 161);
        private static readonly V4Address _destAddress = new V4Address(172, 18, 225, 166);

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
            Assert.Equal(64240, tcpHeader.WindowSize);
            Assert.Equal(_synSequenceNumber, tcpHeader.SequenceNumber);
            Assert.Equal(0u, tcpHeader.AcknowledgmentNumber);
            Assert.Equal(8, tcpHeader.DataOffset);
        }

        [Fact]
        public void TryConsumeSynPacket()
        {
            var span = _tcpSynPacketWithOptions.AsSpan();

            Assert.True(Tcp.TryConsume(span, out var tcp, out var options, out var data));
            Assert.Equal(12, options.Length);
            Assert.Equal(0, data.Length);
        }

        [Fact]
        public void TryConsumeSynPacketWithOptions()
        {
            var span = _tcpSynPacketWithOptions.AsSpan();

            Assert.True(TcpHeaderWithOptions.TryConsume(span, out var header, out var data));
            Assert.True(header.SackPermitted);
            Assert.Equal(1460, header.MaximumSegmentSize);
            Assert.Equal(8, header.WindowScale);
        }

        [Fact]
        public void CanWriteTcpSyn()
        {
            var span = Enumerable.Repeat<byte>(0xFF, 32).ToArray().AsSpan();
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
            tcpHeader.DataOffset = 8;
            tcpHeader.WindowSize = 64240;

            var pseudoHeader = new TcpV4PseudoHeader()
            {
                Destination = _destAddress,
                Source = _sourceAddress,
                ProtocolNumber = ProtocolNumber.Tcp,
                Reserved = 0,
                Size = 32,
            };
            var temp = Checksum.PartialCalculate(ref Unsafe.As<TcpV4PseudoHeader, byte>(ref pseudoHeader), Unsafe.SizeOf<TcpV4PseudoHeader>());
            tcpHeader.Checksum = Checksum.Calculate(ref MemoryMarshal.GetReference(span), 32, temp);

            for (var i = 0; i < _tcpSynPacket.Length;i++)
            {
                Assert.Equal(_tcpSynPacket[i], span[i]);
            }

            Assert.Equal(_tcpSynPacket, span.ToArray());
        }
    }
}

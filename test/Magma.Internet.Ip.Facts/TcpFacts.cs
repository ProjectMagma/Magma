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
        private static readonly byte[] _tcpSynPacket = _tcpSynPacketWithOptions.AsSpan().Slice(0, 20).ToArray();
        private static readonly uint _synSequenceNumber = 3588415412;
        private static readonly V4Address _sourceAddress = new V4Address(192, 168, 1, 104);
        private static readonly V4Address _destAddress = new V4Address(216, 18, 166, 136);

        private static readonly byte[] _tcpActualDataPacket = "ED-79-1B-58-40-7F-8B-F0-21-45-3E-A7-50-18-04-05-D8-8C-00-00-47-45-54-20-2F-20-48-54-54-50-2F-31-2E-31-0D-0A-55-73-65-72-2D-41-67-65-6E-74-3A-20-57-67-65-74-2F-31-2E-31-37-2E-31-20-28-6C-69-6E-75-78-2D-67-6E-75-29-0D-0A-41-63-63-65-70-74-3A-20-2A-2F-2A-0D-0A-41-63-63-65-70-74-2D-45-6E-63-6F-64-69-6E-67-3A-20-69-64-65-6E-74-69-74-79-0D-0A-48-6F-73-74-3A-20-31-37-32-2E-31-38-2E-32-32-35-2E-31-36-36-3A-37-30-30-30-0D-0A-43-6F-6E-6E-65-63-74-69-6F-6E-3A-20-4B-65-65-70-2D-41-6C-69-76-65-0D-0A-0D-0A".HexToByteArray();

        [Fact]
        public void BadPacket()
        {
            Assert.True(TcpHeaderWithOptions.TryConsume(_tcpActualDataPacket.AsSpan(), out var header, out var data));
        }

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
            };

            var temp = Checksum.PartialCalculate(ref Unsafe.As<TcpV4PseudoHeader, byte>(ref pseudoHeader), Unsafe.SizeOf<TcpV4PseudoHeader>());
            tcpHeader.SetChecksum(span, temp);

            Assert.Equal(_tcpSynPacket, span.Slice(0, 20).ToArray());
        }
    }
}

using System;
using Magma.Transport.Tcp.Header;
using Xunit;

namespace Magma.Transport.Tcp.Facts
{
    public class TcpOptionsFacts
    {
        private static readonly byte[] _tcpSynPacketWithOptions = "c2 c3 00 50 d5 e2 df b4 00 00 00 00 a0 02 20 00 c4 47 00 00 02 04 05 b4 01 03 03 02 04 02 08 0a 00 04 aa 62 00 00 00 00".HexToByteArray();

        [Fact]
        public void TryConsumeSynPacketWithOptions()
        {
            var span = _tcpSynPacketWithOptions.AsSpan();

            Assert.True(TcpHeaderWithOptions.TryConsume(span, out var header, out var data));
            Assert.True(header.SackPermitted);
            Assert.Equal(1460, header.MaximumSegmentSize);
            Assert.Equal(2, header.WindowScale);
            Assert.Equal(0, data.Length);
        }

        [Fact]
        public void TcpOptionsParseMaximumSegmentSize()
        {
            var span = _tcpSynPacketWithOptions.AsSpan();

            Assert.True(TcpHeaderWithOptions.TryConsume(span, out var header, out var data));
            Assert.Equal(1460, header.MaximumSegmentSize);
        }

        [Fact]
        public void TcpOptionsParseWindowScale()
        {
            var span = _tcpSynPacketWithOptions.AsSpan();

            Assert.True(TcpHeaderWithOptions.TryConsume(span, out var header, out var data));
            Assert.Equal(2, header.WindowScale);
        }

        [Fact]
        public void TcpOptionsParseSackPermitted()
        {
            var span = _tcpSynPacketWithOptions.AsSpan();

            Assert.True(TcpHeaderWithOptions.TryConsume(span, out var header, out var data));
            Assert.True(header.SackPermitted);
        }

        [Fact]
        public void TcpOptionsParseTimestamp()
        {
            var span = _tcpSynPacketWithOptions.AsSpan();

            Assert.True(TcpHeaderWithOptions.TryConsume(span, out var header, out var data));
            Assert.Equal(0x0004aa62u, header.TimeStamp);
        }

        [Fact]
        public void TcpOptionKindValues()
        {
            Assert.Equal(0, (int)TcpOptionKind.EndOfOptions);
            Assert.Equal(1, (int)TcpOptionKind.NoOp);
            Assert.Equal(2, (int)TcpOptionKind.MaximumSegmentSize);
            Assert.Equal(3, (int)TcpOptionKind.WindowScale);
            Assert.Equal(4, (int)TcpOptionKind.SackPermitted);
            Assert.Equal(8, (int)TcpOptionKind.Timestamps);
        }

        [Fact]
        public void TcpHeaderWithoutOptions()
        {
            var tcpPacket = "c2 c3 00 50 d5 e2 df b4 00 00 00 00 50 02 20 00 00 00 00 00".HexToByteArray();
            var span = tcpPacket.AsSpan();

            Assert.True(TcpHeaderWithOptions.TryConsume(span, out var header, out var data));
            Assert.False(header.SackPermitted);
            Assert.Equal(0, header.MaximumSegmentSize);
            Assert.Equal(0, header.WindowScale);
            Assert.Equal(0, data.Length);
        }

        [Fact]
        public void TcpHeaderWithData()
        {
            var tcpPacket = "c2 c3 00 50 d5 e2 df b5 00 00 00 02 50 18 20 00 00 00 00 00 48 45 4c 4c 4f".HexToByteArray();
            var span = tcpPacket.AsSpan();

            Assert.True(TcpHeaderWithOptions.TryConsume(span, out var header, out var data));
            Assert.Equal(5, data.Length);
            Assert.Equal("HELLO", System.Text.Encoding.ASCII.GetString(data));
        }

        [Fact]
        public void TcpHeaderSizeConstants()
        {
            Assert.Equal(20, TcpHeaderWithOptions.SizeOfStandardHeader);
            Assert.True(TcpHeaderWithOptions.SizeOfSynAckHeader > 20);
        }

        [Fact]
        public void TryConsumeInvalidPacket()
        {
            var invalidPacket = new byte[10];
            var span = invalidPacket.AsSpan();

            Assert.False(TcpHeaderWithOptions.TryConsume(span, out var header, out var data));
        }

        [Fact]
        public void TcpOptionsWithMultipleNoOps()
        {
            var tcpPacketWithNoOps = "c2 c3 00 50 d5 e2 df b4 00 00 00 00 80 02 20 00 00 00 00 00 01 01 02 04 05 b4 00 00 00 00 00 00".HexToByteArray();
            var span = tcpPacketWithNoOps.AsSpan();

            Assert.True(TcpHeaderWithOptions.TryConsume(span, out var header, out var data));
            Assert.Equal(1460, header.MaximumSegmentSize);
        }

        [Fact]
        public void TcpOptionsWithEndOfOptions()
        {
            var tcpPacketWithEnd = "c2 c3 00 50 d5 e2 df b4 00 00 00 00 60 02 20 00 00 00 00 00 02 04 05 b4 00".HexToByteArray();
            var span = tcpPacketWithEnd.AsSpan();

            Assert.True(TcpHeaderWithOptions.TryConsume(span, out var header, out var data));
            Assert.Equal(1460, header.MaximumSegmentSize);
        }
    }
}

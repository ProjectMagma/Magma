using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using Magma.Network.Header;
using Magma.Transport.Tcp.Header;
using Xunit;

namespace Magma.Internet.Ip.Facts
{
    public class TcpFacts
    {
        private static readonly ushort _sourcePort = 57598;
        private static readonly ushort _destPort = 6667;
        private static readonly string _tcpSynPacket = "e0 fe 1a 0b 7b 2b 85 36 00 00 00 00 80 02 fa f0 1b 94 00 00 02 04 05 b4 01 03 03 08 01 01 04 02";
        private static readonly uint _synSequenceNumber = 0x36852b7b;

        [Fact]
        public void CanReadTcpSyn()
        {
            var span = _tcpSynPacket.HexToByteArray().AsSpan();

            var tcpHeader = Unsafe.As<byte, Tcp>(ref MemoryMarshal.GetReference(span));

            Assert.Equal(_sourcePort, tcpHeader.SourcePort);
            Assert.Equal(_destPort, tcpHeader.DestinationPort);
            Assert.Equal(0, tcpHeader.UrgentPointer);
            Assert.True(tcpHeader.SYN);
            Assert.False(tcpHeader.ACK);
            Assert.False(tcpHeader.CWR);
            Assert.Equal(_synSequenceNumber, tcpHeader.SequenceNumber);
            Assert.Equal(0u, tcpHeader.AcknowledgmentNumber);
            Assert.Equal(8, tcpHeader.DataOffset);
        }

        [Fact]
        public void TryConsumeSynPacket()
        {
            var span = _tcpSynPacket.HexToByteArray().AsSpan();

            Assert.True(Tcp.TryConsume(span, out var tcp, out var options, out var data));
            Assert.Equal(12, options.Length);
            Assert.Equal(0, data.Length);
        }

        [Fact]
        public void TryConsumeSynPacketWithOptions()
        {
            var span = _tcpSynPacket.HexToByteArray().AsSpan();

            Assert.True(TcpHeaderWithOptions.TryConsume(span, out var header, out var data));
            Assert.True(header.SackPermitted);
            Assert.Equal(1460, header.MaximumSegmentSize);
            Assert.Equal(8, header.WindowScale);
        }
    }
}

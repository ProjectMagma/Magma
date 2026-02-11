using System;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Magma.Internet.Ip;
using Magma.Network;
using Magma.Transport.Tcp.Header;
using Xunit;
using static Magma.Network.IPAddress;
using TcpHeader = Magma.Network.Header.Tcp;

namespace Magma.Transport.Tcp.Facts
{
    public class TcpHeaderFacts
    {
        private static readonly ushort _sourcePort = 49859;
        private static readonly ushort _destPort = 80;
        private static readonly uint _synSequenceNumber = 3588415412;
        private static readonly V4Address _sourceAddress = new V4Address(192, 168, 1, 104);
        private static readonly V4Address _destAddress = new V4Address(216, 18, 166, 136);

        private static readonly byte[] _tcpSynPacket = "c2 c3 00 50 d5 e2 df b4 00 00 00 00 a0 02 20 00 c4 47 00 00".HexToByteArray();
        private static readonly byte[] _tcpSynPacketWithOptions = "c2 c3 00 50 d5 e2 df b4 00 00 00 00 a0 02 20 00 c4 47 00 00 02 04 05 b4 01 03 03 02 04 02 08 0a 00 04 aa 62 00 00 00 00".HexToByteArray();
        private static readonly byte[] _tcpSynAckPacket = "00 50 c2 c3 00 00 00 01 d5 e2 df b5 a0 12 20 00 00 00 00 00".HexToByteArray();
        private static readonly byte[] _tcpAckPacket = "c2 c3 00 50 d5 e2 df b5 00 00 00 02 50 10 20 00 00 00 00 00".HexToByteArray();
        private static readonly byte[] _tcpFinPacket = "c2 c3 00 50 d5 e2 df b5 00 00 00 02 50 11 20 00 00 00 00 00".HexToByteArray();
        private static readonly byte[] _tcpRstPacket = "c2 c3 00 50 d5 e2 df b5 00 00 00 02 50 14 20 00 00 00 00 00".HexToByteArray();

        [Fact]
        public void CanReadTcpSynHeader()
        {
            var span = _tcpSynPacket.AsSpan();
            ref var tcpHeader = ref Unsafe.As<byte, TcpHeader>(ref MemoryMarshal.GetReference(span));

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
            Assert.False(tcpHeader.FIN);
            Assert.Equal(8192, tcpHeader.WindowSize);
            Assert.Equal(_synSequenceNumber, tcpHeader.SequenceNumber);
            Assert.Equal(0u, tcpHeader.AcknowledgmentNumber);
            Assert.Equal(10, tcpHeader.DataOffset);
        }

        [Fact]
        public void CanReadTcpSynAckHeader()
        {
            var span = _tcpSynAckPacket.AsSpan();
            ref var tcpHeader = ref Unsafe.As<byte, TcpHeader>(ref MemoryMarshal.GetReference(span));

            Assert.Equal(_destPort, tcpHeader.SourcePort);
            Assert.Equal(_sourcePort, tcpHeader.DestinationPort);
            Assert.True(tcpHeader.SYN);
            Assert.True(tcpHeader.ACK);
            Assert.False(tcpHeader.FIN);
            Assert.False(tcpHeader.RST);
            Assert.Equal(1u, tcpHeader.SequenceNumber);
            Assert.Equal(_synSequenceNumber + 1, tcpHeader.AcknowledgmentNumber);
        }

        [Fact]
        public void CanReadTcpAckHeader()
        {
            var span = _tcpAckPacket.AsSpan();
            ref var tcpHeader = ref Unsafe.As<byte, TcpHeader>(ref MemoryMarshal.GetReference(span));

            Assert.True(tcpHeader.ACK);
            Assert.False(tcpHeader.SYN);
            Assert.False(tcpHeader.FIN);
            Assert.False(tcpHeader.RST);
            Assert.Equal(_synSequenceNumber + 1, tcpHeader.SequenceNumber);
            Assert.Equal(2u, tcpHeader.AcknowledgmentNumber);
        }

        [Fact]
        public void CanReadTcpFinHeader()
        {
            var span = _tcpFinPacket.AsSpan();
            ref var tcpHeader = ref Unsafe.As<byte, TcpHeader>(ref MemoryMarshal.GetReference(span));

            Assert.True(tcpHeader.FIN);
            Assert.True(tcpHeader.ACK);
            Assert.False(tcpHeader.SYN);
            Assert.False(tcpHeader.RST);
        }

        [Fact]
        public void CanReadTcpRstHeader()
        {
            var span = _tcpRstPacket.AsSpan();
            ref var tcpHeader = ref Unsafe.As<byte, TcpHeader>(ref MemoryMarshal.GetReference(span));

            Assert.True(tcpHeader.RST);
            Assert.True(tcpHeader.ACK);
            Assert.False(tcpHeader.SYN);
            Assert.False(tcpHeader.FIN);
        }

        [Fact]
        public void CanWriteTcpSynHeader()
        {
            var span = Enumerable.Repeat<byte>(0xFF, 10 * 4).ToArray().AsSpan();
            _tcpSynPacketWithOptions.AsSpan().Slice(20).CopyTo(span.Slice(20));
            ref var tcpHeader = ref Unsafe.As<byte, TcpHeader>(ref MemoryMarshal.GetReference(span));

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

        [Fact]
        public void TryConsumeSynPacket()
        {
            var span = _tcpSynPacketWithOptions.AsSpan();

            Assert.True(TcpHeader.TryConsume(span, out var tcp, out var options, out var data));
            Assert.Equal(20, options.Length);
            Assert.Equal(0, data.Length);
            Assert.Equal(_sourcePort, tcp.SourcePort);
            Assert.Equal(_destPort, tcp.DestinationPort);
        }

        [Fact]
        public void TryConsumeWithInsufficientData()
        {
            var span = _tcpSynPacket.AsSpan().Slice(0, 10);

            Assert.False(TcpHeader.TryConsume(span, out var tcp, out var data));
            Assert.Equal(default, tcp);
            Assert.Equal(0, data.Length);
        }

        [Fact]
        public void TcpFlagsEnumValues()
        {
            Assert.Equal(0b1_0000_0000, (int)TcpFlags.NS);
            Assert.Equal(0b0_1000_0000, (int)TcpFlags.CWR);
            Assert.Equal(0b0_0100_0000, (int)TcpFlags.ECE);
            Assert.Equal(0b0_0010_0000, (int)TcpFlags.URG);
            Assert.Equal(0b0_0001_0000, (int)TcpFlags.ACK);
            Assert.Equal(0b0_0000_1000, (int)TcpFlags.PSH);
            Assert.Equal(0b0_0000_0100, (int)TcpFlags.RST);
            Assert.Equal(0b0_0000_0010, (int)TcpFlags.SYN);
            Assert.Equal(0b0_0000_0001, (int)TcpFlags.FIN);
        }

        [Fact]
        public void CanSetAndGetAllTcpFlags()
        {
            var span = Enumerable.Repeat<byte>(0x00, 20).ToArray().AsSpan();
            ref var tcpHeader = ref Unsafe.As<byte, TcpHeader>(ref MemoryMarshal.GetReference(span));

            tcpHeader.NS = true;
            Assert.True(tcpHeader.NS);
            tcpHeader.NS = false;
            Assert.False(tcpHeader.NS);

            tcpHeader.CWR = true;
            Assert.True(tcpHeader.CWR);
            tcpHeader.CWR = false;
            Assert.False(tcpHeader.CWR);

            tcpHeader.ECE = true;
            Assert.True(tcpHeader.ECE);
            tcpHeader.ECE = false;
            Assert.False(tcpHeader.ECE);

            tcpHeader.URG = true;
            Assert.True(tcpHeader.URG);
            tcpHeader.URG = false;
            Assert.False(tcpHeader.URG);

            tcpHeader.ACK = true;
            Assert.True(tcpHeader.ACK);
            tcpHeader.ACK = false;
            Assert.False(tcpHeader.ACK);

            tcpHeader.PSH = true;
            Assert.True(tcpHeader.PSH);
            tcpHeader.PSH = false;
            Assert.False(tcpHeader.PSH);

            tcpHeader.RST = true;
            Assert.True(tcpHeader.RST);
            tcpHeader.RST = false;
            Assert.False(tcpHeader.RST);

            tcpHeader.SYN = true;
            Assert.True(tcpHeader.SYN);
            tcpHeader.SYN = false;
            Assert.False(tcpHeader.SYN);

            tcpHeader.FIN = true;
            Assert.True(tcpHeader.FIN);
            tcpHeader.FIN = false;
            Assert.False(tcpHeader.FIN);
        }

        [Fact]
        public void CanSetMultipleFlagsSimultaneously()
        {
            var span = Enumerable.Repeat<byte>(0x00, 20).ToArray().AsSpan();
            ref var tcpHeader = ref Unsafe.As<byte, TcpHeader>(ref MemoryMarshal.GetReference(span));

            tcpHeader.SYN = true;
            tcpHeader.ACK = true;

            Assert.True(tcpHeader.SYN);
            Assert.True(tcpHeader.ACK);
            Assert.False(tcpHeader.FIN);
            Assert.False(tcpHeader.RST);
        }

        [Fact]
        public void DataOffsetCalculation()
        {
            var span = Enumerable.Repeat<byte>(0x00, 20).ToArray().AsSpan();
            ref var tcpHeader = ref Unsafe.As<byte, TcpHeader>(ref MemoryMarshal.GetReference(span));

            tcpHeader.DataOffset = 5;
            Assert.Equal(5, tcpHeader.DataOffset);

            tcpHeader.DataOffset = 10;
            Assert.Equal(10, tcpHeader.DataOffset);

            tcpHeader.DataOffset = 15;
            Assert.Equal(15, tcpHeader.DataOffset);
        }

        [Fact]
        public void TcpCreateFactoryMethod()
        {
            var tcp = TcpHeader.Create(8080, 443);

            Assert.Equal(8080, tcp.SourcePort);
            Assert.Equal(443, tcp.DestinationPort);
            Assert.True(tcp.ACK);
            Assert.False(tcp.SYN);
            Assert.False(tcp.FIN);
            Assert.False(tcp.RST);
            Assert.Equal(5, tcp.DataOffset);
        }

        [Fact]
        public void TcpFlagsCombination()
        {
            var flags = TcpFlags.SYN | TcpFlags.ACK;
            Assert.True((flags & TcpFlags.SYN) != 0);
            Assert.True((flags & TcpFlags.ACK) != 0);
            Assert.False((flags & TcpFlags.FIN) != 0);
        }

        [Fact]
        public void TcpWindowSizeEndianness()
        {
            var span = Enumerable.Repeat<byte>(0x00, 20).ToArray().AsSpan();
            ref var tcpHeader = ref Unsafe.As<byte, TcpHeader>(ref MemoryMarshal.GetReference(span));

            tcpHeader.WindowSize = 1024;
            Assert.Equal(1024, tcpHeader.WindowSize);

            tcpHeader.WindowSize = 65535;
            Assert.Equal(65535, tcpHeader.WindowSize);
        }

        [Fact]
        public void TcpPortEndianness()
        {
            var span = Enumerable.Repeat<byte>(0x00, 20).ToArray().AsSpan();
            ref var tcpHeader = ref Unsafe.As<byte, TcpHeader>(ref MemoryMarshal.GetReference(span));

            tcpHeader.SourcePort = 80;
            Assert.Equal(80, tcpHeader.SourcePort);

            tcpHeader.DestinationPort = 443;
            Assert.Equal(443, tcpHeader.DestinationPort);

            tcpHeader.SourcePort = 49152;
            Assert.Equal(49152, tcpHeader.SourcePort);
        }

        [Fact]
        public void TcpSequenceNumberEndianness()
        {
            var span = Enumerable.Repeat<byte>(0x00, 20).ToArray().AsSpan();
            ref var tcpHeader = ref Unsafe.As<byte, TcpHeader>(ref MemoryMarshal.GetReference(span));

            tcpHeader.SequenceNumber = 0x12345678;
            Assert.Equal(0x12345678u, tcpHeader.SequenceNumber);

            tcpHeader.SequenceNumber = uint.MaxValue;
            Assert.Equal(uint.MaxValue, tcpHeader.SequenceNumber);
        }

        [Fact]
        public void TcpAcknowledgmentNumberEndianness()
        {
            var span = Enumerable.Repeat<byte>(0x00, 20).ToArray().AsSpan();
            ref var tcpHeader = ref Unsafe.As<byte, TcpHeader>(ref MemoryMarshal.GetReference(span));

            tcpHeader.AcknowledgmentNumber = 0xABCDEF01;
            Assert.Equal(0xABCDEF01u, tcpHeader.AcknowledgmentNumber);

            tcpHeader.AcknowledgmentNumber = 0;
            Assert.Equal(0u, tcpHeader.AcknowledgmentNumber);
        }

        [Fact]
        public void TcpUrgentPointerEndianness()
        {
            var span = Enumerable.Repeat<byte>(0x00, 20).ToArray().AsSpan();
            ref var tcpHeader = ref Unsafe.As<byte, TcpHeader>(ref MemoryMarshal.GetReference(span));

            tcpHeader.UrgentPointer = 1234;
            Assert.Equal(1234, tcpHeader.UrgentPointer);

            tcpHeader.URG = true;
            Assert.True(tcpHeader.URG);
        }
    }
}

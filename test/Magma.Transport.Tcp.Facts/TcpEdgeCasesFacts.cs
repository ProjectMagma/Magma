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
    public class TcpEdgeCasesFacts
    {
        private static readonly V4Address _sourceAddress = new V4Address(192, 168, 1, 104);
        private static readonly V4Address _destAddress = new V4Address(216, 18, 166, 136);

        [Fact]
        public void ZeroWindowPacket()
        {
            var span = Enumerable.Repeat<byte>(0x00, 20).ToArray().AsSpan();
            ref var tcpHeader = ref Unsafe.As<byte, TcpHeader>(ref MemoryMarshal.GetReference(span));

            tcpHeader.SourcePort = 8080;
            tcpHeader.DestinationPort = 443;
            tcpHeader.SequenceNumber = 1000;
            tcpHeader.AcknowledgmentNumber = 2000;
            tcpHeader.DataOffset = 5;
            tcpHeader.ACK = true;
            tcpHeader.WindowSize = 0;

            Assert.Equal(0, tcpHeader.WindowSize);
            Assert.True(tcpHeader.ACK);
        }

        [Fact]
        public void ZeroWindowProbePacket()
        {
            var span = Enumerable.Repeat<byte>(0x00, 21).ToArray().AsSpan();
            ref var tcpHeader = ref Unsafe.As<byte, TcpHeader>(ref MemoryMarshal.GetReference(span));

            tcpHeader.SourcePort = 8080;
            tcpHeader.DestinationPort = 443;
            tcpHeader.SequenceNumber = 1000;
            tcpHeader.AcknowledgmentNumber = 2000;
            tcpHeader.DataOffset = 5;
            tcpHeader.ACK = true;
            tcpHeader.WindowSize = 0;

            span[20] = 0xFF;

            Assert.True(TcpHeader.TryConsume(span, out var tcp, out var data));
            Assert.Equal(1, data.Length);
            Assert.Equal(0, tcp.WindowSize);
        }

        [Fact]
        public void RstPacketHandling()
        {
            var span = Enumerable.Repeat<byte>(0x00, 20).ToArray().AsSpan();
            ref var tcpHeader = ref Unsafe.As<byte, TcpHeader>(ref MemoryMarshal.GetReference(span));

            tcpHeader.SourcePort = 8080;
            tcpHeader.DestinationPort = 443;
            tcpHeader.SequenceNumber = 1000;
            tcpHeader.AcknowledgmentNumber = 0;
            tcpHeader.DataOffset = 5;
            tcpHeader.RST = true;

            Assert.True(tcpHeader.RST);
            Assert.False(tcpHeader.ACK);
            Assert.False(tcpHeader.SYN);
            Assert.False(tcpHeader.FIN);
        }

        [Fact]
        public void RstAckPacketHandling()
        {
            var span = Enumerable.Repeat<byte>(0x00, 20).ToArray().AsSpan();
            ref var tcpHeader = ref Unsafe.As<byte, TcpHeader>(ref MemoryMarshal.GetReference(span));

            tcpHeader.SourcePort = 8080;
            tcpHeader.DestinationPort = 443;
            tcpHeader.SequenceNumber = 1000;
            tcpHeader.AcknowledgmentNumber = 2000;
            tcpHeader.DataOffset = 5;
            tcpHeader.RST = true;
            tcpHeader.ACK = true;

            Assert.True(tcpHeader.RST);
            Assert.True(tcpHeader.ACK);
        }

        [Fact]
        public void OutOfOrderSegmentSequenceNumber()
        {
            var span1 = Enumerable.Repeat<byte>(0x00, 20).ToArray().AsSpan();
            ref var tcpHeader1 = ref Unsafe.As<byte, TcpHeader>(ref MemoryMarshal.GetReference(span1));
            tcpHeader1.SequenceNumber = 2000;

            var span2 = Enumerable.Repeat<byte>(0x00, 20).ToArray().AsSpan();
            ref var tcpHeader2 = ref Unsafe.As<byte, TcpHeader>(ref MemoryMarshal.GetReference(span2));
            tcpHeader2.SequenceNumber = 1000;

            Assert.True(tcpHeader1.SequenceNumber > tcpHeader2.SequenceNumber);
        }

        [Fact]
        public void DuplicateAckPacket()
        {
            var span1 = Enumerable.Repeat<byte>(0x00, 20).ToArray().AsSpan();
            ref var tcpHeader1 = ref Unsafe.As<byte, TcpHeader>(ref MemoryMarshal.GetReference(span1));
            tcpHeader1.AcknowledgmentNumber = 1000;
            tcpHeader1.ACK = true;

            var span2 = Enumerable.Repeat<byte>(0x00, 20).ToArray().AsSpan();
            ref var tcpHeader2 = ref Unsafe.As<byte, TcpHeader>(ref MemoryMarshal.GetReference(span2));
            tcpHeader2.AcknowledgmentNumber = 1000;
            tcpHeader2.ACK = true;

            Assert.Equal(tcpHeader1.AcknowledgmentNumber, tcpHeader2.AcknowledgmentNumber);
        }

        [Fact]
        public void MaxWindowSize()
        {
            var span = Enumerable.Repeat<byte>(0x00, 20).ToArray().AsSpan();
            ref var tcpHeader = ref Unsafe.As<byte, TcpHeader>(ref MemoryMarshal.GetReference(span));

            tcpHeader.WindowSize = ushort.MaxValue;

            Assert.Equal(ushort.MaxValue, tcpHeader.WindowSize);
        }

        [Fact]
        public void SequenceNumberWrapAround()
        {
            var span = Enumerable.Repeat<byte>(0x00, 20).ToArray().AsSpan();
            ref var tcpHeader = ref Unsafe.As<byte, TcpHeader>(ref MemoryMarshal.GetReference(span));

            tcpHeader.SequenceNumber = uint.MaxValue;
            Assert.Equal(uint.MaxValue, tcpHeader.SequenceNumber);

            unchecked
            {
                var nextSeq = tcpHeader.SequenceNumber + 1;
                Assert.Equal(0u, nextSeq);
            }
        }

        [Fact]
        public void MinimumTcpHeaderSize()
        {
            var span = Enumerable.Repeat<byte>(0x00, 20).ToArray().AsSpan();
            ref var tcpHeader = ref Unsafe.As<byte, TcpHeader>(ref MemoryMarshal.GetReference(span));

            tcpHeader.DataOffset = 5;

            Assert.Equal(5, tcpHeader.DataOffset);
            Assert.Equal(20, tcpHeader.DataOffset * 4);
        }

        [Fact]
        public void MaximumTcpHeaderSize()
        {
            var span = Enumerable.Repeat<byte>(0x00, 60).ToArray().AsSpan();
            ref var tcpHeader = ref Unsafe.As<byte, TcpHeader>(ref MemoryMarshal.GetReference(span));

            tcpHeader.DataOffset = 15;

            Assert.Equal(15, tcpHeader.DataOffset);
            Assert.Equal(60, tcpHeader.DataOffset * 4);
        }

        [Fact]
        public void SynFloodScenario()
        {
            for (var i = 0; i < 100; i++)
            {
                var span = Enumerable.Repeat<byte>(0x00, 20).ToArray().AsSpan();
                ref var tcpHeader = ref Unsafe.As<byte, TcpHeader>(ref MemoryMarshal.GetReference(span));

                tcpHeader.SourcePort = (ushort)(10000 + i);
                tcpHeader.DestinationPort = 80;
                tcpHeader.SequenceNumber = (uint)(i * 1000);
                tcpHeader.DataOffset = 5;
                tcpHeader.SYN = true;

                Assert.True(tcpHeader.SYN);
                Assert.False(tcpHeader.ACK);
            }
        }

        [Fact]
        public void UrgentPointerWithUrgFlag()
        {
            var span = Enumerable.Repeat<byte>(0x00, 20).ToArray().AsSpan();
            ref var tcpHeader = ref Unsafe.As<byte, TcpHeader>(ref MemoryMarshal.GetReference(span));

            tcpHeader.URG = true;
            tcpHeader.UrgentPointer = 100;

            Assert.True(tcpHeader.URG);
            Assert.Equal(100, tcpHeader.UrgentPointer);
        }

        [Fact]
        public void SimultaneousClose()
        {
            var span1 = Enumerable.Repeat<byte>(0x00, 20).ToArray().AsSpan();
            ref var tcpHeader1 = ref Unsafe.As<byte, TcpHeader>(ref MemoryMarshal.GetReference(span1));
            tcpHeader1.FIN = true;
            tcpHeader1.ACK = true;

            var span2 = Enumerable.Repeat<byte>(0x00, 20).ToArray().AsSpan();
            ref var tcpHeader2 = ref Unsafe.As<byte, TcpHeader>(ref MemoryMarshal.GetReference(span2));
            tcpHeader2.FIN = true;
            tcpHeader2.ACK = true;

            Assert.True(tcpHeader1.FIN && tcpHeader1.ACK);
            Assert.True(tcpHeader2.FIN && tcpHeader2.ACK);
        }

        [Fact]
        public void PshFlagWithData()
        {
            var span = Enumerable.Repeat<byte>(0x00, 30).ToArray().AsSpan();
            ref var tcpHeader = ref Unsafe.As<byte, TcpHeader>(ref MemoryMarshal.GetReference(span));

            tcpHeader.DataOffset = 5;
            tcpHeader.PSH = true;
            tcpHeader.ACK = true;

            Assert.True(TcpHeader.TryConsume(span, out var tcp, out var data));
            Assert.Equal(10, data.Length);
            Assert.True(tcp.PSH);
        }

        [Fact]
        public void EcnFlagsHandling()
        {
            var span = Enumerable.Repeat<byte>(0x00, 20).ToArray().AsSpan();
            ref var tcpHeader = ref Unsafe.As<byte, TcpHeader>(ref MemoryMarshal.GetReference(span));

            tcpHeader.ECE = true;
            tcpHeader.CWR = true;

            Assert.True(tcpHeader.ECE);
            Assert.True(tcpHeader.CWR);
        }

        [Fact]
        public void NsEcnNonceFlag()
        {
            var span = Enumerable.Repeat<byte>(0x00, 20).ToArray().AsSpan();
            ref var tcpHeader = ref Unsafe.As<byte, TcpHeader>(ref MemoryMarshal.GetReference(span));

            tcpHeader.NS = true;

            Assert.True(tcpHeader.NS);
        }

        [Fact]
        public void InvalidDataOffsetHandling()
        {
            var span = Enumerable.Repeat<byte>(0x00, 20).ToArray().AsSpan();
            ref var tcpHeader = ref Unsafe.As<byte, TcpHeader>(ref MemoryMarshal.GetReference(span));

            tcpHeader.DataOffset = 3;

            Assert.Equal(3, tcpHeader.DataOffset);
            Assert.Equal(12, tcpHeader.DataOffset * 4);
        }

        [Fact]
        public void ChecksumZeroValue()
        {
            var span = Enumerable.Repeat<byte>(0x00, 20).ToArray().AsSpan();
            ref var tcpHeader = ref Unsafe.As<byte, TcpHeader>(ref MemoryMarshal.GetReference(span));

            tcpHeader.Checksum = 0;

            Assert.Equal(0, tcpHeader.Checksum);
        }

        [Fact]
        public void AllFlagsSet()
        {
            var span = Enumerable.Repeat<byte>(0x00, 20).ToArray().AsSpan();
            ref var tcpHeader = ref Unsafe.As<byte, TcpHeader>(ref MemoryMarshal.GetReference(span));

            tcpHeader.NS = true;
            tcpHeader.CWR = true;
            tcpHeader.ECE = true;
            tcpHeader.URG = true;
            tcpHeader.ACK = true;
            tcpHeader.PSH = true;
            tcpHeader.RST = true;
            tcpHeader.SYN = true;
            tcpHeader.FIN = true;

            Assert.True(tcpHeader.NS);
            Assert.True(tcpHeader.CWR);
            Assert.True(tcpHeader.ECE);
            Assert.True(tcpHeader.URG);
            Assert.True(tcpHeader.ACK);
            Assert.True(tcpHeader.PSH);
            Assert.True(tcpHeader.RST);
            Assert.True(tcpHeader.SYN);
            Assert.True(tcpHeader.FIN);
        }
    }
}

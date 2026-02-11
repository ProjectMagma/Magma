using System;
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
    public class TcpChecksumFacts
    {
        private static readonly V4Address _sourceAddress = new V4Address(192, 168, 1, 104);
        private static readonly V4Address _destAddress = new V4Address(216, 18, 166, 136);
        private static readonly ushort _sourcePort = 49859;
        private static readonly ushort _destPort = 80;
        private static readonly uint _synSequenceNumber = 3588415412;

        private static readonly byte[] _tcpSynPacketWithOptions = "c2 c3 00 50 d5 e2 df b4 00 00 00 00 a0 02 20 00 c4 47 00 00 02 04 05 b4 01 03 03 02 04 02 08 0a 00 04 aa 62 00 00 00 00".HexToByteArray();

        [Fact]
        public void CanCalculateTcpChecksum()
        {
            var span = _tcpSynPacketWithOptions.AsSpan();
            ref var tcpHeader = ref Unsafe.As<byte, TcpHeader>(ref MemoryMarshal.GetReference(span));

            var originalChecksum = tcpHeader.Checksum;

            var pseudoHeader = new TcpV4PseudoHeader()
            {
                Destination = _destAddress,
                Source = _sourceAddress,
                ProtocolNumber = ProtocolNumber.Tcp,
                Reserved = 0,
            };

            var pseudoSum = Checksum.PartialCalculate(ref Unsafe.As<TcpV4PseudoHeader, byte>(ref pseudoHeader), Unsafe.SizeOf<TcpV4PseudoHeader>());
            tcpHeader.Checksum = 0;
            tcpHeader.SetChecksum(span, pseudoSum);

            Assert.Equal(originalChecksum, tcpHeader.Checksum);
        }

        [Fact]
        public void ChecksumValidationWithPseudoHeader()
        {
            var span = _tcpSynPacketWithOptions.AsSpan();

            var pseudoHeader = new TcpV4PseudoHeader()
            {
                Destination = _destAddress,
                Source = _sourceAddress,
                ProtocolNumber = ProtocolNumber.Tcp,
                Reserved = 0,
            };

            var size = (ushort)System.Net.IPAddress.HostToNetworkOrder((short)span.Length);
            var pseudoSum = Checksum.PartialCalculate(ref Unsafe.As<TcpV4PseudoHeader, byte>(ref pseudoHeader), Unsafe.SizeOf<TcpV4PseudoHeader>());
            pseudoSum = Checksum.PartialCalculate(ref Unsafe.As<ushort, byte>(ref size), sizeof(ushort), pseudoSum);

            var checksumResult = Checksum.Calculate(ref MemoryMarshal.GetReference(span), span.Length, pseudoSum);

            Assert.Equal(0, checksumResult);
        }

        [Fact]
        public void CanCalculateChecksumForEmptyData()
        {
            var span = new byte[20];
            ref var tcpHeader = ref Unsafe.As<byte, TcpHeader>(ref MemoryMarshal.GetReference(span.AsSpan()));

            tcpHeader.SourcePort = _sourcePort;
            tcpHeader.DestinationPort = _destPort;
            tcpHeader.SequenceNumber = _synSequenceNumber;
            tcpHeader.AcknowledgmentNumber = 0;
            tcpHeader.DataOffset = 5;
            tcpHeader.SYN = true;
            tcpHeader.WindowSize = 8192;
            tcpHeader.Checksum = 0;

            var pseudoHeader = new TcpV4PseudoHeader()
            {
                Destination = _destAddress,
                Source = _sourceAddress,
                ProtocolNumber = ProtocolNumber.Tcp,
                Reserved = 0,
            };

            var pseudoSum = Checksum.PartialCalculate(ref Unsafe.As<TcpV4PseudoHeader, byte>(ref pseudoHeader), Unsafe.SizeOf<TcpV4PseudoHeader>());
            tcpHeader.SetChecksum(span.AsSpan(), pseudoSum);

            Assert.NotEqual(0, tcpHeader.Checksum);
        }

        [Fact]
        public void PartialChecksumCalculation()
        {
            var data = new byte[100];
            for (var i = 0; i < data.Length; i++)
            {
                data[i] = (byte)(i % 256);
            }

            var fullChecksum = Checksum.Calculate(ref MemoryMarshal.GetReference(data.AsSpan()), data.Length);

            var partial1 = Checksum.PartialCalculate(ref MemoryMarshal.GetReference(data.AsSpan()), 50);
            var partial2Checksum = Checksum.Calculate(ref MemoryMarshal.GetReference(data.AsSpan(50)), 50, partial1);

            Assert.Equal(fullChecksum, partial2Checksum);
        }

        [Fact]
        public void ChecksumWithData()
        {
            var headerSize = 20;
            var dataSize = 100;
            var totalSize = headerSize + dataSize;
            var span = new byte[totalSize];

            ref var tcpHeader = ref Unsafe.As<byte, TcpHeader>(ref MemoryMarshal.GetReference(span.AsSpan()));
            tcpHeader.SourcePort = 8080;
            tcpHeader.DestinationPort = 443;
            tcpHeader.SequenceNumber = 1000;
            tcpHeader.AcknowledgmentNumber = 2000;
            tcpHeader.DataOffset = 5;
            tcpHeader.ACK = true;
            tcpHeader.PSH = true;
            tcpHeader.WindowSize = 1024;
            tcpHeader.Checksum = 0;

            for (var i = headerSize; i < totalSize; i++)
            {
                span[i] = (byte)(i % 256);
            }

            var pseudoHeader = new TcpV4PseudoHeader()
            {
                Source = new V4Address(10, 0, 0, 1),
                Destination = new V4Address(10, 0, 0, 2),
                ProtocolNumber = ProtocolNumber.Tcp,
                Reserved = 0,
            };

            var pseudoSum = Checksum.PartialCalculate(ref Unsafe.As<TcpV4PseudoHeader, byte>(ref pseudoHeader), Unsafe.SizeOf<TcpV4PseudoHeader>());
            tcpHeader.SetChecksum(span.AsSpan(), pseudoSum);

            Assert.NotEqual(0, tcpHeader.Checksum);

            var size = (ushort)System.Net.IPAddress.HostToNetworkOrder((short)span.Length);
            pseudoSum = Checksum.PartialCalculate(ref Unsafe.As<TcpV4PseudoHeader, byte>(ref pseudoHeader), Unsafe.SizeOf<TcpV4PseudoHeader>());
            pseudoSum = Checksum.PartialCalculate(ref Unsafe.As<ushort, byte>(ref size), sizeof(ushort), pseudoSum);

            var checksumResult = Checksum.Calculate(ref MemoryMarshal.GetReference(span.AsSpan()), span.Length, pseudoSum);
            Assert.Equal(0, checksumResult);
        }

        [Fact]
        public void PseudoHeaderStructSize()
        {
            Assert.Equal(10, Unsafe.SizeOf<TcpV4PseudoHeader>());
        }

        [Fact]
        public void PseudoHeaderFields()
        {
            var pseudoHeader = new TcpV4PseudoHeader()
            {
                Source = _sourceAddress,
                Destination = _destAddress,
                ProtocolNumber = ProtocolNumber.Tcp,
                Reserved = 0,
            };

            Assert.Equal(_sourceAddress, pseudoHeader.Source);
            Assert.Equal(_destAddress, pseudoHeader.Destination);
            Assert.Equal(ProtocolNumber.Tcp, pseudoHeader.ProtocolNumber);
            Assert.Equal(0, pseudoHeader.Reserved);
        }
    }
}

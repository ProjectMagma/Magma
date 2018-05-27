using System;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Magma.Network.Header;
using Xunit;
using static Magma.Network.IPAddress;

namespace Magma.Internet.Ip.Facts
{
    public class IPFacts
    {
        private static readonly string _ipHeader = "45 00 00 34 1f a2 40 00 80 06 bf b4 ac 12 e1 a1 ac 12 e1 a6";
        private static readonly V4Address _sourceAddress = new V4Address(172,18,225,161);
        private static readonly V4Address _destAddress = new V4Address(172,18,225,166);

        [Fact]
        public void CanReadIPHeader()
        {
            var array = _ipHeader.HexToByteArray().AsSpan();

            var ipHeader = Unsafe.As<byte, IPv4>(ref MemoryMarshal.GetReference(array));

            Assert.Equal(4, ipHeader.Version);
            Assert.Equal(5, ipHeader.InternetHeaderLength);
            Assert.Equal(20, ipHeader.HeaderLength);
            Assert.Equal(_sourceAddress, ipHeader.SourceAddress);
            Assert.Equal(_destAddress, ipHeader.DestinationAddress);
            Assert.True(ipHeader.DontFragment);
            Assert.Equal(ProtocolNumber.Tcp, ipHeader.Protocol);
            Assert.Equal(32, ipHeader.DataLength);
            Assert.Equal(20, ipHeader.HeaderLength);
            Assert.Equal(52, ipHeader.TotalLength);
            Assert.Equal(41503, ipHeader.Identification);
        }

        [Fact]
        public void WriteIPHeader()
        {
            var span = (Enumerable.Repeat<byte>(0xFF, 20).ToArray()).AsSpan();
            //Fill it with junk
            
            ref var ipHeader = ref Unsafe.As<byte, IPv4>(ref MemoryMarshal.GetReference(span));
            IPv4.InitHeader(ref ipHeader, _sourceAddress, _destAddress, 32, ProtocolNumber.Tcp, 41503);
            
            Assert.True(_ipHeader.HexToByteArray().AsSpan().SequenceEqual(span));
        }

        [Fact]
        public void InternetHeaderLength()
        {
            var ipHeader = new IPv4()
            {
                InternetHeaderLength = 5,
            };

            Assert.Equal(5, ipHeader.InternetHeaderLength);
            Assert.Equal(20, ipHeader.HeaderLength);
        }

        [Fact]
        public void HeaderLength()
        {
            var ipHeader = new IPv4()
            {
                HeaderLength = 20,
            };

            Assert.Equal(5, ipHeader.InternetHeaderLength);
            Assert.Equal(20, ipHeader.HeaderLength);
        }

        [Fact]
        public void HeaderAndVersionWorkTogether()
        {
            var ipHeader = new IPv4()
            {
                InternetHeaderLength = 5,
                Version = 4,
            };

            Assert.Equal(4, ipHeader.Version);
            Assert.Equal(5, ipHeader.InternetHeaderLength);
        }
    }
}

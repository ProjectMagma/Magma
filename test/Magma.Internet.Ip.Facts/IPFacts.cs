using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Magma.Network.Header;
using Xunit;

namespace Magma.Internet.Ip.Facts
{
    public class IPFacts
    {
        private static readonly string _ipHeader = "45 00 00 34 1f a2 40 00 80 06 00 00 ac 12 e1 a1 ac 12 e1 a6";

        [Fact]
        public void CanReadIPHeader()
        {
            var array = _ipHeader.HexToByteArray().AsSpan();

            var ipHeader = Unsafe.As<byte, IPv4>(ref MemoryMarshal.GetReference(array));

            Assert.Equal(4, ipHeader.Version);
            Assert.Equal(5, ipHeader.InternetHeaderLength);
            Assert.Equal(20, ipHeader.HeaderLength);
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

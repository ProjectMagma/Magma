using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Magma.Network.Header;
using Xunit;

namespace Magma.Internet.Ip.Facts
{
    public class IPFacts
    {
        private static readonly string _ipHeader = "45 00 00 34 1f a2 40 00 80 06 00 00 ac 12 e1 a1 ac 12 e1 a6"; // cd 11 1a 0b 7e 03 d3 f1 00 00 00 00 80 02 fa f0 1b 94 00 00 02 04 05 b4 01 03 03 08 01 01 04 02";

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
    }
}

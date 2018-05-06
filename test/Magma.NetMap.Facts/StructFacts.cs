using System;
using System.Runtime.CompilerServices;
using Magma.NetMap.Interop;
using Xunit;

namespace Magma.NetMap.Facts
{
    public class StructFacts
    {
        [Fact]
        public void CheckDescriptionSizeMatchesNative()
        {
            var expectedSize = 752;
            var actualSize = Unsafe.SizeOf<nm_desc>();
            Assert.Equal(expectedSize, actualSize);
        }

        [Fact]
        public void CheckRequestSizeMatchesNative()
        {
            var expectedSize = 60;
            var actual = Unsafe.SizeOf<nmreq>();
            Assert.Equal(expectedSize, actual);
        }
    }
}

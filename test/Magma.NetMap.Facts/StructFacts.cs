using System;
using System.Runtime.CompilerServices;
using Magma.NetMap.Interop;
using Xunit;

namespace Magma.NetMap.Facts
{
    public class StructFacts
    {
        [Fact]
        public void CheckRequestSizeMatchesNative()
        {
            var expectedSize = 60;
            var actual = Unsafe.SizeOf<NetMapRequest>();
            Assert.Equal(expectedSize, actual);
        }
    }
}

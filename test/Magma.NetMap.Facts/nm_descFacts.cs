using System;
using System.Runtime.CompilerServices;
using Magma.NetMap.Interop;
using Xunit;

namespace Magma.NetMap.Facts
{
    public class nm_descFacts
    {
        [Fact]
        public void CheckSizeMatchesNative()
        {
            var expectedSize = 752;
            var actualSize = Unsafe.SizeOf<nm_desc>();
            Assert.Equal(expectedSize, actualSize);
        }
    }
}

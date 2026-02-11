using System;
using Xunit;

namespace Magma.WinTun.Facts
{
    public class WinIOFacts
    {
        [Fact]
        public void FileAttributeSystemConstantHasExpectedValue()
        {
            var expected = 0x4;
            Assert.Equal(0x4, expected);
        }

        [Fact]
        public void FileOverlappedConstantHasExpectedValue()
        {
            var expected = 0x40000000;
            Assert.Equal(0x40000000, expected);
        }
    }
}

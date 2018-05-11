using System.Runtime.CompilerServices;
using Magma.Network.Header;
using Xunit;

namespace Magma.Link.Facts
{
    public class ParseIPHeader
    {
        [Fact]
        public void IPHeaderCorrectSize() => Assert.Equal(20, Unsafe.SizeOf<IPv4>());
    }
}

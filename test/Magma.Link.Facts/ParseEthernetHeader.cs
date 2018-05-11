using System;
using Xunit;

namespace Magma.Link.Facts
{
    public class ParseEthernetHeader
    {
        private const string ValidFrame = "00-15-5D-F3-43-11-82-15-F5-19-E6-94-08-00-45-00-00-4C-15-DF-00-00-80-06-A0-69-AC-1E-96-11-AC-1E-96-15-F8-06-00-16-7D-73-6D-5B-C9-25-AE-7E-50-18-20-14-27-43-00-00-C8-48-2D-EE-E4-89-04-C5-56-4B-04-51-C5-C7-08-8D-47-06-E6-1E-EF-E1-D8-BE-FB-AF-BD-83-BB-36-A0-3B-3F-73-37-07";
        
        [Fact]
        public void EthernetLengthCorrect()
        {
            var frame = ValidFrame.HexToByteArray().AsSpan();

            Assert.True(Network.Header.Ethernet.TryConsume(frame, out var ethernet, out var data));

            Assert.Equal(72, data.Length);
        }

        [Fact]
        public void FromMacAddressCorrect()
        {
            var frame = ValidFrame.HexToByteArray().AsSpan();
            Assert.True(Network.Header.Ethernet.TryConsume(frame, out var ethernet, out var data));

        }
    }
}

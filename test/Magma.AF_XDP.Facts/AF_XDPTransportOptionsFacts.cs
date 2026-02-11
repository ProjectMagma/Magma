using System;
using Magma.AF_XDP;
using Xunit;

namespace Magma.AF_XDP.Facts
{
    public class AF_XDPTransportOptionsFacts
    {
        [Fact]
        public void DefaultValuesAreCorrect()
        {
            var options = new AF_XDPTransportOptions();

            Assert.Equal(0, options.QueueId);
            Assert.True(options.UseZeroCopy);
            Assert.Equal(4096, options.UmemFrameCount);
            Assert.Equal(2048, options.FrameSize);
            Assert.Equal(2048, options.RxRingSize);
            Assert.Equal(2048, options.TxRingSize);
        }

        [Fact]
        public void CanSetInterfaceName()
        {
            var options = new AF_XDPTransportOptions
            {
                InterfaceName = "eth0"
            };

            Assert.Equal("eth0", options.InterfaceName);
        }

        [Fact]
        public void CanSetQueueId()
        {
            var options = new AF_XDPTransportOptions
            {
                QueueId = 5
            };

            Assert.Equal(5, options.QueueId);
        }

        [Fact]
        public void CanDisableZeroCopy()
        {
            var options = new AF_XDPTransportOptions
            {
                UseZeroCopy = false
            };

            Assert.False(options.UseZeroCopy);
        }

        [Fact]
        public void CanSetUmemFrameCount()
        {
            var options = new AF_XDPTransportOptions
            {
                UmemFrameCount = 8192
            };

            Assert.Equal(8192, options.UmemFrameCount);
        }

        [Fact]
        public void CanSetFrameSize()
        {
            var options = new AF_XDPTransportOptions
            {
                FrameSize = 4096
            };

            Assert.Equal(4096, options.FrameSize);
        }

        [Fact]
        public void CanSetRingSizes()
        {
            var options = new AF_XDPTransportOptions
            {
                RxRingSize = 4096,
                TxRingSize = 1024
            };

            Assert.Equal(4096, options.RxRingSize);
            Assert.Equal(1024, options.TxRingSize);
        }
    }
}

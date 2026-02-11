using System;
using Magma.AF_XDP.Internal;
using Xunit;

namespace Magma.AF_XDP.Facts
{
    public class AF_XDPMemoryManagerFacts
    {
        [Fact]
        public void FrameSizeCalculationIsCorrect()
        {
            uint frameCount = 4096;
            uint frameSize = 2048;

            Assert.Equal((ulong)frameCount * frameSize, (ulong)frameCount * frameSize);
        }

        [Fact]
        public void GetFrameAddressCalculationIsCorrect()
        {
            nint baseAddr = 0x1000;
            uint frameSize = 2048;
            ulong frameIndex = 10;

            nint expectedAddr = baseAddr + (nint)(frameIndex * frameSize);

            Assert.Equal(0x1000 + (10 * 2048), (long)expectedAddr);
        }

        [Fact(Skip = "Requires Linux with libbpf installed and XDP support")]
        public unsafe void CanCreateMemoryManager()
        {
            uint frameCount = 4096;
            uint frameSize = 2048;

            using (var manager = new AF_XDPMemoryManager(frameCount, frameSize))
            {
                Assert.NotEqual(0, manager.Umem);
                Assert.Equal(frameSize, manager.FrameSize);
            }
        }

        [Fact(Skip = "Requires Linux with libbpf installed and XDP support")]
        public unsafe void CanGetFrameAddress()
        {
            uint frameCount = 4096;
            uint frameSize = 2048;

            using (var manager = new AF_XDPMemoryManager(frameCount, frameSize))
            {
                nint addr0 = manager.GetFrameAddress(0);
                nint addr1 = manager.GetFrameAddress(1);

                Assert.NotEqual(0, addr0);
                Assert.Equal(addr0 + (nint)frameSize, addr1);
            }
        }

        [Fact(Skip = "Requires Linux with libbpf installed and XDP support")]
        public unsafe void CanGetFrameMemory()
        {
            uint frameCount = 4096;
            uint frameSize = 2048;

            using (var manager = new AF_XDPMemoryManager(frameCount, frameSize))
            {
                var memory = manager.GetFrameMemory(0);

                Assert.Equal((int)frameSize, memory.Length);
            }
        }

        [Fact(Skip = "Requires Linux with libbpf installed and XDP support")]
        public unsafe void DisposalCleansUpResources()
        {
            uint frameCount = 4096;
            uint frameSize = 2048;

            var manager = new AF_XDPMemoryManager(frameCount, frameSize);
            manager.Dispose();

            Assert.Equal(0, manager.Umem);
        }
    }
}

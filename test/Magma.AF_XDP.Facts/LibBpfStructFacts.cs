using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Magma.AF_XDP.Interop;
using Xunit;

namespace Magma.AF_XDP.Facts
{
    public class LibBpfStructFacts
    {
        [Fact]
        public void XskUmemConfigHasCorrectSize()
        {
            var size = Unsafe.SizeOf<LibBpf.xsk_umem_config>();
            Assert.Equal(20, size);
        }

        [Fact]
        public void XskSocketConfigHasCorrectSize()
        {
            var size = Unsafe.SizeOf<LibBpf.xsk_socket_config>();
            Assert.Equal(20, size);
        }

        [Fact]
        public void XskRingProdHasCorrectSize()
        {
            var size = Unsafe.SizeOf<LibBpf.xsk_ring_prod>();
            Assert.True(size > 0);
        }

        [Fact]
        public void XskRingConsHasCorrectSize()
        {
            var size = Unsafe.SizeOf<LibBpf.xsk_ring_cons>();
            Assert.True(size > 0);
        }

        [Fact]
        public void XdpDescHasCorrectSize()
        {
            var size = Unsafe.SizeOf<LibBpf.xdp_desc>();
            Assert.Equal(16, size);
        }

        [Fact]
        public void XskBindFlagsHasCorrectValues()
        {
            Assert.Equal(1, (int)LibBpf.XskBindFlags.XDP_ZEROCOPY);
            Assert.Equal(2, (int)LibBpf.XskBindFlags.XDP_COPY);
            Assert.Equal(8, (int)LibBpf.XskBindFlags.XDP_USE_NEED_WAKEUP);
        }

        [Fact]
        public void CanCreateXskUmemConfig()
        {
            var config = new LibBpf.xsk_umem_config
            {
                fill_size = 2048,
                comp_size = 2048,
                frame_size = 2048,
                frame_headroom = 0,
                flags = 0
            };

            Assert.Equal(2048u, config.fill_size);
            Assert.Equal(2048u, config.comp_size);
            Assert.Equal(2048u, config.frame_size);
            Assert.Equal(0u, config.frame_headroom);
            Assert.Equal(0u, config.flags);
        }

        [Fact]
        public void CanCreateXskSocketConfig()
        {
            var config = new LibBpf.xsk_socket_config
            {
                rx_size = 2048,
                tx_size = 2048,
                libbpf_flags = 0,
                xdp_flags = 0,
                bind_flags = (ushort)LibBpf.XskBindFlags.XDP_ZEROCOPY
            };

            Assert.Equal(2048u, config.rx_size);
            Assert.Equal(2048u, config.tx_size);
            Assert.Equal(0u, config.libbpf_flags);
            Assert.Equal(0u, config.xdp_flags);
            Assert.Equal((ushort)LibBpf.XskBindFlags.XDP_ZEROCOPY, config.bind_flags);
        }

        [Fact]
        public void CanCreateXdpDesc()
        {
            var desc = new LibBpf.xdp_desc
            {
                addr = 0x1000,
                len = 1500,
                options = 0
            };

            Assert.Equal(0x1000ul, desc.addr);
            Assert.Equal(1500u, desc.len);
            Assert.Equal(0u, desc.options);
        }
    }
}

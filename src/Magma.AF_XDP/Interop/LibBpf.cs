using System;
using System.Runtime.InteropServices;

namespace Magma.AF_XDP.Interop
{
    /// <summary>
    /// P/Invoke declarations for libbpf XDP socket functions
    /// </summary>
    public static class LibBpf
    {
        private const string LibBpfName = "libbpf.so.1";

        // XDP socket creation flags
        [Flags]
        public enum XskBindFlags : ushort
        {
            XDP_ZEROCOPY = 1 << 0,
            XDP_COPY = 1 << 1,
            XDP_USE_NEED_WAKEUP = 1 << 3,
        }

        // UMEM configuration
        [StructLayout(LayoutKind.Sequential)]
        public struct xsk_umem_config
        {
            public uint fill_size;
            public uint comp_size;
            public uint frame_size;
            public uint frame_headroom;
            public uint flags;
        }

        // Socket configuration
        [StructLayout(LayoutKind.Sequential)]
        public struct xsk_socket_config
        {
            public uint rx_size;
            public uint tx_size;
            public uint libbpf_flags;
            public uint xdp_flags;
            public ushort bind_flags;
        }

        // Ring structures (opaque pointers managed by libbpf)
        [StructLayout(LayoutKind.Sequential)]
        public struct xsk_ring_prod
        {
            public nint cached_prod;
            public nint cached_cons;
            public nint mask;
            public nint size;
            public nint producer;
            public nint consumer;
            public nint ring;
            public nint flags;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct xsk_ring_cons
        {
            public nint cached_prod;
            public nint cached_cons;
            public nint mask;
            public nint size;
            public nint producer;
            public nint consumer;
            public nint ring;
            public nint flags;
        }

        // TX descriptor structure
        [StructLayout(LayoutKind.Sequential)]
        public struct xdp_desc
        {
            public ulong addr;
            public uint len;
            public uint options;
        }

        // UMEM functions
        [DllImport(LibBpfName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int xsk_umem__create(
            out nint umem,
            nint umem_area,
            ulong size,
            ref xsk_ring_prod fill,
            ref xsk_ring_cons comp,
            ref xsk_umem_config config);

        [DllImport(LibBpfName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int xsk_umem__delete(nint umem);

        // Socket functions
        [DllImport(LibBpfName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern int xsk_socket__create(
            out nint xsk,
            string ifname,
            uint queue_id,
            nint umem,
            ref xsk_ring_cons rx,
            ref xsk_ring_prod tx,
            ref xsk_socket_config config);

        [DllImport(LibBpfName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int xsk_socket__delete(nint xsk);

        [DllImport(LibBpfName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int xsk_socket__fd(nint xsk);

        // Ring operations - producer (TX/Fill)
        [DllImport(LibBpfName, CallingConvention = CallingConvention.Cdecl)]
        public static extern uint xsk_ring_prod__reserve(ref xsk_ring_prod prod, uint nb, out uint idx);

        [DllImport(LibBpfName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void xsk_ring_prod__submit(ref xsk_ring_prod prod, uint nb);

        [DllImport(LibBpfName, CallingConvention = CallingConvention.Cdecl)]
        public static extern nint xsk_ring_prod__fill_addr(ref xsk_ring_prod fill, uint idx);

        [DllImport(LibBpfName, CallingConvention = CallingConvention.Cdecl)]
        public static extern nint xsk_ring_prod__tx_desc(ref xsk_ring_prod tx, uint idx);

        [DllImport(LibBpfName, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool xsk_ring_prod__needs_wakeup(ref xsk_ring_prod prod);

        // Ring operations - consumer (RX/Completion)
        [DllImport(LibBpfName, CallingConvention = CallingConvention.Cdecl)]
        public static extern uint xsk_ring_cons__peek(ref xsk_ring_cons cons, uint nb, out uint idx);

        [DllImport(LibBpfName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void xsk_ring_cons__release(ref xsk_ring_cons cons, uint nb);

        [DllImport(LibBpfName, CallingConvention = CallingConvention.Cdecl)]
        public static extern nint xsk_ring_cons__rx_desc(ref xsk_ring_cons rx, uint idx);

        [DllImport(LibBpfName, CallingConvention = CallingConvention.Cdecl)]
        public static extern nint xsk_ring_cons__comp_addr(ref xsk_ring_cons comp, uint idx);

        // Helper for sendto (for wakeup)
        [DllImport("libc.so.6", CallingConvention = CallingConvention.Cdecl)]
        public static extern int sendto(int sockfd, nint buf, int len, int flags, nint dest_addr, int addrlen);

        public const int MSG_DONTWAIT = 0x40;
    }
}

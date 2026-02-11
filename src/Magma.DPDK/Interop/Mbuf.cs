using System;
using System.Runtime.InteropServices;

namespace Magma.DPDK.Interop
{
    /// <summary>
    /// P/Invoke declarations for DPDK memory pool (mbuf) management
    /// </summary>
    public static class Mbuf
    {
        private const string LibDpdk = "librte_mbuf.so";

        /// <summary>
        /// Mbuf structure (simplified view for P/Invoke).
        /// The actual structure is more complex, but we only need the pointer.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct rte_mbuf
        {
            public nint buf_addr;
            public ulong buf_iova;
            public ushort data_off;
            public ushort refcnt;
            public ushort nb_segs;
            public ushort port;
            public ulong ol_flags;
            public uint packet_type;
            public uint pkt_len;
            public ushort data_len;
            public ushort vlan_tci;
        }

        /// <summary>
        /// Create a packet mbuf pool.
        /// </summary>
        /// <param name="name">Name of the mbuf pool</param>
        /// <param name="n">Number of elements in the pool</param>
        /// <param name="cache_size">Size of per-lcore cache</param>
        /// <param name="priv_size">Size of application private data</param>
        /// <param name="data_room_size">Size of data buffer in each mbuf</param>
        /// <param name="socket_id">NUMA socket to allocate memory from</param>
        /// <returns>Pointer to mbuf pool structure, or NULL on error</returns>
        [DllImport(LibDpdk, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern nint rte_pktmbuf_pool_create(
            string name,
            uint n,
            uint cache_size,
            ushort priv_size,
            ushort data_room_size,
            int socket_id);

        /// <summary>
        /// Free a packet mbuf pool.
        /// </summary>
        /// <param name="mp">Pointer to mbuf pool</param>
        [DllImport(LibDpdk, CallingConvention = CallingConvention.Cdecl)]
        public static extern void rte_mempool_free(nint mp);

        /// <summary>
        /// Allocate a new mbuf from a mempool.
        /// </summary>
        /// <param name="mp">Pointer to mbuf pool</param>
        /// <returns>Pointer to mbuf, or NULL on error</returns>
        [DllImport(LibDpdk, CallingConvention = CallingConvention.Cdecl)]
        public static extern nint rte_pktmbuf_alloc(nint mp);

        /// <summary>
        /// Free a packet mbuf back to its mempool.
        /// </summary>
        /// <param name="m">Pointer to mbuf</param>
        [DllImport(LibDpdk, CallingConvention = CallingConvention.Cdecl)]
        public static extern void rte_pktmbuf_free(nint m);

        /// <summary>
        /// Get the data pointer from an mbuf.
        /// </summary>
        /// <param name="m">Pointer to mbuf</param>
        /// <returns>Pointer to data buffer</returns>
        public static unsafe nint rte_pktmbuf_mtod(nint m)
        {
            if (m == nint.Zero)
                return nint.Zero;

            var mbuf = Marshal.PtrToStructure<rte_mbuf>(m);
            return mbuf.buf_addr + mbuf.data_off;
        }

        /// <summary>
        /// Prepend data to the beginning of an mbuf.
        /// </summary>
        /// <param name="m">Pointer to mbuf</param>
        /// <param name="len">Amount of data to prepend</param>
        /// <returns>Pointer to prepended data, or NULL on error</returns>
        [DllImport(LibDpdk, CallingConvention = CallingConvention.Cdecl)]
        public static extern nint rte_pktmbuf_prepend(nint m, ushort len);

        /// <summary>
        /// Append data to the end of an mbuf.
        /// </summary>
        /// <param name="m">Pointer to mbuf</param>
        /// <param name="len">Amount of data to append</param>
        /// <returns>Pointer to appended data, or NULL on error</returns>
        [DllImport(LibDpdk, CallingConvention = CallingConvention.Cdecl)]
        public static extern nint rte_pktmbuf_append(nint m, ushort len);

        /// <summary>
        /// Remove data at the beginning of an mbuf.
        /// </summary>
        /// <param name="m">Pointer to mbuf</param>
        /// <param name="len">Amount of data to remove</param>
        /// <returns>Pointer to new data start, or NULL on error</returns>
        [DllImport(LibDpdk, CallingConvention = CallingConvention.Cdecl)]
        public static extern nint rte_pktmbuf_adj(nint m, ushort len);

        /// <summary>
        /// Remove data at the end of an mbuf.
        /// </summary>
        /// <param name="m">Pointer to mbuf</param>
        /// <param name="len">Amount of data to remove</param>
        /// <returns>0 on success, negative on error</returns>
        [DllImport(LibDpdk, CallingConvention = CallingConvention.Cdecl)]
        public static extern int rte_pktmbuf_trim(nint m, ushort len);

        /// <summary>
        /// Reset the fields of a packet mbuf to their default values.
        /// </summary>
        /// <param name="m">Pointer to mbuf</param>
        [DllImport(LibDpdk, CallingConvention = CallingConvention.Cdecl)]
        public static extern void rte_pktmbuf_reset(nint m);

        /// <summary>
        /// Get the headroom in a packet mbuf.
        /// </summary>
        /// <param name="m">Pointer to mbuf</param>
        /// <returns>Amount of headroom in bytes</returns>
        public static ushort rte_pktmbuf_headroom(nint m)
        {
            if (m == nint.Zero)
                return 0;

            var mbuf = Marshal.PtrToStructure<rte_mbuf>(m);
            return mbuf.data_off;
        }

        /// <summary>
        /// Common mbuf sizes.
        /// </summary>
        public const ushort RTE_MBUF_DEFAULT_BUF_SIZE = 2048 + 128;
        public const ushort RTE_MBUF_DEFAULT_DATAROOM = 2048;
        public const int RTE_MEMPOOL_CACHE_MAX_SIZE = 512;
        public const int SOCKET_ID_ANY = -1;
    }
}

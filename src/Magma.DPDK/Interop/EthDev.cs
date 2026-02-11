using System;
using System.Runtime.InteropServices;

namespace Magma.DPDK.Interop
{
    /// <summary>
    /// P/Invoke declarations for DPDK Ethernet device management
    /// </summary>
    public static class EthDev
    {
        private const string LibDpdk = "rte_ethdev";

        /// <summary>
        /// Ethernet device configuration structure.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct rte_eth_conf
        {
            public uint link_speeds;
            public rte_eth_rxmode rxmode;
            public rte_eth_txmode txmode;
            public uint lpbk_mode;
            public rte_eth_dcb_rx_conf rx_adv_conf_dcb;
            public rte_eth_vmdq_dcb_conf vmdq_dcb_conf;
            public rte_eth_dcb_tx_conf dcb_tx_conf;
            public rte_eth_vmdq_tx_conf vmdq_tx_conf;
        }

        /// <summary>
        /// Ethernet RX mode configuration.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct rte_eth_rxmode
        {
            public uint mq_mode;
            public uint mtu;
            public uint max_lro_pkt_size;
            public ushort split_hdr_size;
            public ulong offloads;
            public uint reserved_64s0;
            public uint reserved_64s1;
            public uint reserved_ptrs0;
            public uint reserved_ptrs1;
        }

        /// <summary>
        /// Ethernet TX mode configuration.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct rte_eth_txmode
        {
            public uint mq_mode;
            public ulong offloads;
            public ushort pvid;
            public byte hw_vlan_reject_tagged;
            public byte hw_vlan_reject_untagged;
            public byte hw_vlan_insert_pvid;
            public uint reserved_64s;
            public uint reserved_ptrs;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct rte_eth_dcb_rx_conf
        {
            public uint nb_tcs;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
            public byte[] dcb_tc;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct rte_eth_vmdq_dcb_conf
        {
            public uint nb_queue_pools;
            public uint enable_default_pool;
            public byte default_pool;
            public byte nb_pool_maps;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct rte_eth_dcb_tx_conf
        {
            public uint nb_tcs;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
            public byte[] dcb_tc;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct rte_eth_vmdq_tx_conf
        {
            public uint nb_queue_pools;
        }

        /// <summary>
        /// RX queue configuration structure.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct rte_eth_rxconf
        {
            public rte_eth_thresh rx_thresh;
            public ushort rx_free_thresh;
            public byte rx_drop_en;
            public byte rx_deferred_start;
            public ulong offloads;
        }

        /// <summary>
        /// TX queue configuration structure.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct rte_eth_txconf
        {
            public rte_eth_thresh tx_thresh;
            public ushort tx_rs_thresh;
            public ushort tx_free_thresh;
            public byte tx_deferred_start;
            public ulong offloads;
        }

        /// <summary>
        /// Threshold values for RX/TX queues.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct rte_eth_thresh
        {
            public byte pthresh;
            public byte hthresh;
            public byte wthresh;
        }

        /// <summary>
        /// Get the number of available Ethernet devices.
        /// </summary>
        /// <returns>The number of available Ethernet devices</returns>
        [DllImport(LibDpdk, CallingConvention = CallingConvention.Cdecl)]
        public static extern ushort rte_eth_dev_count_avail();

        /// <summary>
        /// Configure an Ethernet device.
        /// </summary>
        /// <param name="port_id">The port identifier</param>
        /// <param name="nb_rx_queue">Number of RX queues</param>
        /// <param name="nb_tx_queue">Number of TX queues</param>
        /// <param name="eth_conf">Device configuration</param>
        /// <returns>0 on success, negative error code on failure</returns>
        [DllImport(LibDpdk, CallingConvention = CallingConvention.Cdecl)]
        public static extern int rte_eth_dev_configure(ushort port_id, ushort nb_rx_queue, ushort nb_tx_queue, ref rte_eth_conf eth_conf);

        /// <summary>
        /// Allocate and set up a receive queue for an Ethernet device.
        /// </summary>
        /// <param name="port_id">The port identifier</param>
        /// <param name="rx_queue_id">The RX queue index (must be in range [0, nb_rx_queue-1])</param>
        /// <param name="nb_rx_desc">Number of receive descriptors</param>
        /// <param name="socket_id">NUMA socket ID for memory allocation</param>
        /// <param name="rx_conf">RX queue configuration, or NULL for default</param>
        /// <param name="mb_pool">Memory pool from which to allocate mbufs</param>
        /// <returns>0 on success, negative error code on failure</returns>
        [DllImport(LibDpdk, CallingConvention = CallingConvention.Cdecl)]
        public static extern int rte_eth_rx_queue_setup(ushort port_id, ushort rx_queue_id, ushort nb_rx_desc, uint socket_id, ref rte_eth_rxconf rx_conf, nint mb_pool);

        /// <summary>
        /// Allocate and set up a transmit queue for an Ethernet device.
        /// </summary>
        /// <param name="port_id">The port identifier</param>
        /// <param name="tx_queue_id">The TX queue index (must be in range [0, nb_tx_queue-1])</param>
        /// <param name="nb_tx_desc">Number of transmit descriptors</param>
        /// <param name="socket_id">NUMA socket ID for memory allocation</param>
        /// <param name="tx_conf">TX queue configuration, or NULL for default</param>
        /// <returns>0 on success, negative error code on failure</returns>
        [DllImport(LibDpdk, CallingConvention = CallingConvention.Cdecl)]
        public static extern int rte_eth_tx_queue_setup(ushort port_id, ushort tx_queue_id, ushort nb_tx_desc, uint socket_id, ref rte_eth_txconf tx_conf);

        /// <summary>
        /// Start an Ethernet device.
        /// </summary>
        /// <param name="port_id">The port identifier</param>
        /// <returns>0 on success, negative error code on failure</returns>
        [DllImport(LibDpdk, CallingConvention = CallingConvention.Cdecl)]
        public static extern int rte_eth_dev_start(ushort port_id);

        /// <summary>
        /// Stop an Ethernet device.
        /// </summary>
        /// <param name="port_id">The port identifier</param>
        /// <returns>0 on success, negative error code on failure</returns>
        [DllImport(LibDpdk, CallingConvention = CallingConvention.Cdecl)]
        public static extern int rte_eth_dev_stop(ushort port_id);

        /// <summary>
        /// Close a stopped Ethernet device.
        /// </summary>
        /// <param name="port_id">The port identifier</param>
        /// <returns>0 on success, negative error code on failure</returns>
        [DllImport(LibDpdk, CallingConvention = CallingConvention.Cdecl)]
        public static extern int rte_eth_dev_close(ushort port_id);

        /// <summary>
        /// Enable receipt in promiscuous mode for an Ethernet device.
        /// </summary>
        /// <param name="port_id">The port identifier</param>
        /// <returns>0 on success, negative error code on failure</returns>
        [DllImport(LibDpdk, CallingConvention = CallingConvention.Cdecl)]
        public static extern int rte_eth_promiscuous_enable(ushort port_id);

        /// <summary>
        /// Disable receipt in promiscuous mode for an Ethernet device.
        /// </summary>
        /// <param name="port_id">The port identifier</param>
        /// <returns>0 on success, negative error code on failure</returns>
        [DllImport(LibDpdk, CallingConvention = CallingConvention.Cdecl)]
        public static extern int rte_eth_promiscuous_disable(ushort port_id);

        /// <summary>
        /// Retrieve the Ethernet address of an Ethernet device.
        /// </summary>
        /// <param name="port_id">The port identifier</param>
        /// <param name="mac_addr">Pointer to buffer to store MAC address</param>
        /// <returns>0 on success, negative error code on failure</returns>
        [DllImport(LibDpdk, CallingConvention = CallingConvention.Cdecl)]
        public static extern int rte_eth_macaddr_get(ushort port_id, nint mac_addr);
    }
}

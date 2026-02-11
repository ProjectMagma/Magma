using System.Runtime.InteropServices;

namespace Magma.DPDK.Interop;

/// <summary>
/// DPDK mbuf (memory buffer) structure for packet data.
/// This is a simplified version containing the essential fields needed for basic RX/TX operations.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
internal unsafe struct rte_mbuf
{
    public void* buf_addr;
    public ulong buf_iova;
    
    // Rearm data marker
    public ushort data_off;
    public ushort refcnt;
    public ushort nb_segs;
    public ushort port;
    
    public ulong ol_flags;
    
    // Packet type and RSS hash
    public uint packet_type;
    public uint pkt_len;
    public ushort data_len;
    public ushort vlan_tci;
    
    public ulong hash_rss;
    
    public ushort vlan_tci_outer;
    public ushort buf_len;
    
    // Pool pointer
    public void* pool;
    
    // Second cache line - skip for now
    // We'll access packet data via buf_addr + data_off
}

/// <summary>
/// DPDK Ethernet device configuration structure.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
internal struct rte_eth_conf
{
    public uint link_speeds;
    public rte_eth_rxmode rxmode;
    public rte_eth_txmode txmode;
    public uint lpbk_mode;
    public rte_eth_rxconf rxconf;
    public rte_eth_txconf txconf;
}

[StructLayout(LayoutKind.Sequential)]
internal struct rte_eth_rxmode
{
    public ulong offloads;
    public uint mtu;
    public ushort max_lro_pkt_size;
    public ushort split_hdr_size;
}

[StructLayout(LayoutKind.Sequential)]
internal struct rte_eth_txmode
{
    public ulong offloads;
    public ushort pvid;
    public byte hw_vlan_reject_tagged;
    public byte hw_vlan_reject_untagged;
    public byte hw_vlan_insert_pvid;
}

[StructLayout(LayoutKind.Sequential)]
internal struct rte_eth_rxconf
{
    public ushort rx_thresh_pthresh;
    public ushort rx_thresh_hthresh;
    public ushort rx_thresh_wthresh;
    public byte rx_drop_en;
    public byte rx_deferred_start;
    public ulong offloads;
}

[StructLayout(LayoutKind.Sequential)]
internal struct rte_eth_txconf
{
    public ushort tx_thresh_pthresh;
    public ushort tx_thresh_hthresh;
    public ushort tx_thresh_wthresh;
    public ushort tx_rs_thresh;
    public ushort tx_free_thresh;
    public byte tx_deferred_start;
    public ulong offloads;
}

/// <summary>
/// Link speeds
/// </summary>
internal static class RteEthLinkSpeed
{
    public const uint RTE_ETH_LINK_SPEED_AUTONEG = 0;
    public const uint RTE_ETH_LINK_SPEED_FIXED = 0x80000000;
    public const uint RTE_ETH_LINK_SPEED_10M_HD = 0x00000001;
    public const uint RTE_ETH_LINK_SPEED_10M = 0x00000002;
    public const uint RTE_ETH_LINK_SPEED_100M_HD = 0x00000004;
    public const uint RTE_ETH_LINK_SPEED_100M = 0x00000008;
    public const uint RTE_ETH_LINK_SPEED_1G = 0x00000010;
    public const uint RTE_ETH_LINK_SPEED_2_5G = 0x00000020;
    public const uint RTE_ETH_LINK_SPEED_5G = 0x00000040;
    public const uint RTE_ETH_LINK_SPEED_10G = 0x00000080;
    public const uint RTE_ETH_LINK_SPEED_20G = 0x00000100;
    public const uint RTE_ETH_LINK_SPEED_25G = 0x00000200;
    public const uint RTE_ETH_LINK_SPEED_40G = 0x00000400;
    public const uint RTE_ETH_LINK_SPEED_50G = 0x00000800;
    public const uint RTE_ETH_LINK_SPEED_56G = 0x00001000;
    public const uint RTE_ETH_LINK_SPEED_100G = 0x00002000;
    public const uint RTE_ETH_LINK_SPEED_200G = 0x00004000;
}

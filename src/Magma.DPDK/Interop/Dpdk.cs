using System.Runtime.InteropServices;

namespace Magma.DPDK.Interop;

/// <summary>
/// P/Invoke declarations for DPDK (Data Plane Development Kit) functions.
/// These bindings provide access to DPDK's low-level packet I/O operations.
/// </summary>
internal static unsafe class Dpdk
{
    private const string LibraryName = "librte_ethdev.so";

    /// <summary>
    /// Initialize the EAL (Environment Abstraction Layer).
    /// Must be called before any other DPDK function.
    /// </summary>
    /// <param name="argc">Argument count</param>
    /// <param name="argv">Argument vector</param>
    /// <returns>Number of parsed arguments on success, negative on error</returns>
    [DllImport("librte_eal.so", CallingConvention = CallingConvention.Cdecl)]
    public static extern int rte_eal_init(int argc, byte** argv);

    /// <summary>
    /// Get the number of Ethernet devices available.
    /// </summary>
    /// <returns>Number of available Ethernet devices</returns>
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
    public static extern ushort rte_eth_dev_count_avail();

    /// <summary>
    /// Configure an Ethernet device.
    /// </summary>
    /// <param name="port_id">Port identifier</param>
    /// <param name="nb_rx_queue">Number of RX queues</param>
    /// <param name="nb_tx_queue">Number of TX queues</param>
    /// <param name="eth_conf">Device configuration</param>
    /// <returns>0 on success, negative on error</returns>
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
    public static extern int rte_eth_dev_configure(ushort port_id, ushort nb_rx_queue, ushort nb_tx_queue, ref rte_eth_conf eth_conf);

    /// <summary>
    /// Allocate and set up a receive queue for an Ethernet device.
    /// </summary>
    /// <param name="port_id">Port identifier</param>
    /// <param name="rx_queue_id">RX queue index (must be in range [0, nb_rx_queue - 1])</param>
    /// <param name="nb_rx_desc">Number of RX descriptors</param>
    /// <param name="socket_id">NUMA socket ID for memory allocation</param>
    /// <param name="rx_conf">RX queue configuration (can be null for defaults)</param>
    /// <param name="mb_pool">Memory pool from which to allocate rte_mbuf network memory buffers</param>
    /// <returns>0 on success, negative on error</returns>
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
    public static extern int rte_eth_rx_queue_setup(ushort port_id, ushort rx_queue_id, ushort nb_rx_desc, uint socket_id, rte_eth_rxconf* rx_conf, void* mb_pool);

    /// <summary>
    /// Allocate and set up a transmit queue for an Ethernet device.
    /// </summary>
    /// <param name="port_id">Port identifier</param>
    /// <param name="tx_queue_id">TX queue index (must be in range [0, nb_tx_queue - 1])</param>
    /// <param name="nb_tx_desc">Number of TX descriptors</param>
    /// <param name="socket_id">NUMA socket ID for memory allocation</param>
    /// <param name="tx_conf">TX queue configuration (can be null for defaults)</param>
    /// <returns>0 on success, negative on error</returns>
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
    public static extern int rte_eth_tx_queue_setup(ushort port_id, ushort tx_queue_id, ushort nb_tx_desc, uint socket_id, rte_eth_txconf* tx_conf);

    /// <summary>
    /// Start an Ethernet device.
    /// The device start step is the last one before beginning to receive and transmit packets.
    /// </summary>
    /// <param name="port_id">Port identifier</param>
    /// <returns>0 on success, negative on error</returns>
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
    public static extern int rte_eth_dev_start(ushort port_id);

    /// <summary>
    /// Stop an Ethernet device. The device can be restarted with rte_eth_dev_start().
    /// </summary>
    /// <param name="port_id">Port identifier</param>
    /// <returns>0 on success, negative on error</returns>
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
    public static extern int rte_eth_dev_stop(ushort port_id);

    /// <summary>
    /// Close a stopped Ethernet device and release resources.
    /// </summary>
    /// <param name="port_id">Port identifier</param>
    /// <returns>0 on success, negative on error</returns>
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
    public static extern int rte_eth_dev_close(ushort port_id);

    /// <summary>
    /// Enable receipt in promiscuous mode for an Ethernet device.
    /// </summary>
    /// <param name="port_id">Port identifier</param>
    /// <returns>0 on success, negative on error</returns>
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
    public static extern int rte_eth_promiscuous_enable(ushort port_id);

    /// <summary>
    /// Disable receipt in promiscuous mode for an Ethernet device.
    /// </summary>
    /// <param name="port_id">Port identifier</param>
    /// <returns>0 on success, negative on error</returns>
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
    public static extern int rte_eth_promiscuous_disable(ushort port_id);

    /// <summary>
    /// Retrieve a burst of input packets from an RX queue.
    /// This is the main function for receiving packets in poll mode.
    /// </summary>
    /// <param name="port_id">Port identifier</param>
    /// <param name="queue_id">RX queue index (must be in range [0, nb_rx_queue - 1])</param>
    /// <param name="rx_pkts">Array of pointers to rte_mbuf structures for received packets</param>
    /// <param name="nb_pkts">Maximum number of packets to retrieve</param>
    /// <returns>Number of packets actually retrieved (0 to nb_pkts)</returns>
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
    public static extern ushort rte_eth_rx_burst(ushort port_id, ushort queue_id, rte_mbuf** rx_pkts, ushort nb_pkts);

    /// <summary>
    /// Send a burst of output packets on a TX queue.
    /// This is the main function for transmitting packets in poll mode.
    /// </summary>
    /// <param name="port_id">Port identifier</param>
    /// <param name="queue_id">TX queue index (must be in range [0, nb_tx_queue - 1])</param>
    /// <param name="tx_pkts">Array of pointers to rte_mbuf structures for packets to send</param>
    /// <param name="nb_pkts">Number of packets to transmit</param>
    /// <returns>Number of packets actually sent</returns>
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
    public static extern ushort rte_eth_tx_burst(ushort port_id, ushort queue_id, rte_mbuf** tx_pkts, ushort nb_pkts);

    /// <summary>
    /// Create a new mbuf pool.
    /// </summary>
    /// <param name="name">Name of the mbuf pool</param>
    /// <param name="n">Number of elements in the pool</param>
    /// <param name="cache_size">Size of the per-core object cache</param>
    /// <param name="priv_size">Size of application private data</param>
    /// <param name="data_room_size">Size of data buffer in each mbuf</param>
    /// <param name="socket_id">NUMA socket ID</param>
    /// <returns>Pointer to the new mbuf pool, or null on error</returns>
    [DllImport("librte_mbuf.so", CallingConvention = CallingConvention.Cdecl)]
    public static extern void* rte_pktmbuf_pool_create(byte* name, uint n, uint cache_size, ushort priv_size, ushort data_room_size, int socket_id);

    /// <summary>
    /// Free a packet mbuf back to its pool.
    /// </summary>
    /// <param name="m">Pointer to the mbuf to free</param>
    [DllImport("librte_mbuf.so", CallingConvention = CallingConvention.Cdecl)]
    public static extern void rte_pktmbuf_free(rte_mbuf* m);

    /// <summary>
    /// Allocate a new mbuf from a pool.
    /// </summary>
    /// <param name="mp">Pointer to the mbuf pool</param>
    /// <returns>Pointer to the new mbuf, or null on error</returns>
    [DllImport("librte_mbuf.so", CallingConvention = CallingConvention.Cdecl)]
    public static extern rte_mbuf* rte_pktmbuf_alloc(void* mp);

    /// <summary>
    /// Get the NUMA socket ID for a given lcore.
    /// </summary>
    /// <param name="lcore_id">Logical core ID</param>
    /// <returns>NUMA socket ID</returns>
    [DllImport("librte_eal.so", CallingConvention = CallingConvention.Cdecl)]
    public static extern uint rte_lcore_to_socket_id(uint lcore_id);

    /// <summary>
    /// Get the ID of the current lcore.
    /// </summary>
    /// <returns>Current lcore ID</returns>
    [DllImport("librte_eal.so", CallingConvention = CallingConvention.Cdecl)]
    public static extern uint rte_lcore_id();

    /// <summary>
    /// Special socket ID value indicating "any socket" for NUMA-unaware allocation.
    /// </summary>
    public const int SOCKET_ID_ANY = -1;
}

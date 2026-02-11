using System;
using System.Runtime.InteropServices;

namespace Magma.DPDK.Interop
{
    /// <summary>
    /// P/Invoke declarations for DPDK Environment Abstraction Layer (EAL)
    /// </summary>
    public static class EAL
    {
        private const string LibDpdk = "librte_eal.so";

        /// <summary>
        /// Initialize the Environment Abstraction Layer (EAL).
        /// This function must be called before any other DPDK function.
        /// </summary>
        /// <param name="argc">Number of arguments in argv array</param>
        /// <param name="argv">Array of arguments (program name + EAL arguments)</param>
        /// <returns>
        /// The number of arguments parsed on success, or negative error code on failure.
        /// The parsed arguments are consumed and removed from argv.
        /// </returns>
        [DllImport(LibDpdk, CallingConvention = CallingConvention.Cdecl)]
        public static extern int rte_eal_init(int argc, [In] string[] argv);

        /// <summary>
        /// Clean up the Environment Abstraction Layer (EAL).
        /// This function must be called to release any resources allocated by rte_eal_init().
        /// </summary>
        /// <returns>0 on success, negative error code on failure</returns>
        [DllImport(LibDpdk, CallingConvention = CallingConvention.Cdecl)]
        public static extern int rte_eal_cleanup();

        /// <summary>
        /// Get the current lcore ID.
        /// </summary>
        /// <returns>The current lcore ID</returns>
        [DllImport(LibDpdk, CallingConvention = CallingConvention.Cdecl)]
        public static extern uint rte_lcore_id();

        /// <summary>
        /// Get the total number of enabled lcores.
        /// </summary>
        /// <returns>The number of enabled lcores</returns>
        [DllImport(LibDpdk, CallingConvention = CallingConvention.Cdecl)]
        public static extern uint rte_lcore_count();

        /// <summary>
        /// Get the ID of the main lcore.
        /// </summary>
        /// <returns>The ID of the main lcore</returns>
        [DllImport(LibDpdk, CallingConvention = CallingConvention.Cdecl)]
        public static extern uint rte_get_main_lcore();

        /// <summary>
        /// Check if the specified lcore is enabled.
        /// </summary>
        /// <param name="lcore_id">The lcore ID to check</param>
        /// <returns>1 if the lcore is enabled, 0 otherwise</returns>
        [DllImport(LibDpdk, CallingConvention = CallingConvention.Cdecl)]
        public static extern int rte_lcore_is_enabled(uint lcore_id);

        /// <summary>
        /// Get the socket ID (NUMA node) of the specified lcore.
        /// </summary>
        /// <param name="lcore_id">The lcore ID</param>
        /// <returns>The socket ID, or -1 on error</returns>
        [DllImport(LibDpdk, CallingConvention = CallingConvention.Cdecl)]
        public static extern int rte_lcore_to_socket_id(uint lcore_id);

        /// <summary>
        /// Return the Application thread ID of the execution unit.
        /// </summary>
        /// <param name="lcore_id">The lcore ID</param>
        /// <returns>The pthread ID, or -1 on error</returns>
        [DllImport(LibDpdk, CallingConvention = CallingConvention.Cdecl)]
        public static extern int rte_lcore_to_cpu_id(uint lcore_id);
    }
}

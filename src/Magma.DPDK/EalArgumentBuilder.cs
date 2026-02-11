using System;
using System.Collections.Generic;
using System.Text;

namespace Magma.DPDK
{
    /// <summary>
    /// Builder for constructing DPDK EAL (Environment Abstraction Layer) arguments.
    /// </summary>
    public class EalArgumentBuilder
    {
        private readonly List<string> _arguments = new List<string>();
        private string _programName = "magma-dpdk";

        /// <summary>
        /// Creates a new EAL argument builder with default settings.
        /// </summary>
        public EalArgumentBuilder()
        {
        }

        /// <summary>
        /// Sets the program name (argv[0]).
        /// </summary>
        /// <param name="name">Program name</param>
        /// <returns>This builder for method chaining</returns>
        public EalArgumentBuilder WithProgramName(string name)
        {
            _programName = name ?? throw new ArgumentNullException(nameof(name));
            return this;
        }

        /// <summary>
        /// Sets the core mask for lcores to run on.
        /// </summary>
        /// <param name="coreMask">Hexadecimal bitmask of cores (e.g., "0xF" for cores 0-3)</param>
        /// <returns>This builder for method chaining</returns>
        public EalArgumentBuilder WithCoreMask(string coreMask)
        {
            if (string.IsNullOrWhiteSpace(coreMask))
                throw new ArgumentException("Core mask cannot be null or whitespace", nameof(coreMask));

            _arguments.Add("-c");
            _arguments.Add(coreMask);
            return this;
        }

        /// <summary>
        /// Sets the list of cores to run on.
        /// </summary>
        /// <param name="coreList">Comma-separated list of cores (e.g., "0,2,4" or "0-3,8")</param>
        /// <returns>This builder for method chaining</returns>
        public EalArgumentBuilder WithCoreList(string coreList)
        {
            if (string.IsNullOrWhiteSpace(coreList))
                throw new ArgumentException("Core list cannot be null or whitespace", nameof(coreList));

            _arguments.Add("-l");
            _arguments.Add(coreList);
            return this;
        }

        /// <summary>
        /// Sets the number of memory channels.
        /// </summary>
        /// <param name="channels">Number of memory channels (typically 2, 4, or 8)</param>
        /// <returns>This builder for method chaining</returns>
        public EalArgumentBuilder WithMemoryChannels(int channels)
        {
            if (channels <= 0)
                throw new ArgumentOutOfRangeException(nameof(channels), "Memory channels must be positive");

            _arguments.Add("-n");
            _arguments.Add(channels.ToString());
            return this;
        }

        /// <summary>
        /// Sets the amount of memory in megabytes.
        /// </summary>
        /// <param name="megabytes">Amount of memory in MB</param>
        /// <returns>This builder for method chaining</returns>
        public EalArgumentBuilder WithMemory(int megabytes)
        {
            if (megabytes <= 0)
                throw new ArgumentOutOfRangeException(nameof(megabytes), "Memory must be positive");

            _arguments.Add("-m");
            _arguments.Add(megabytes.ToString());
            return this;
        }

        /// <summary>
        /// Adds a PCI device to the allowlist (whitelist).
        /// </summary>
        /// <param name="pciAddress">PCI address (e.g., "0000:01:00.0")</param>
        /// <returns>This builder for method chaining</returns>
        public EalArgumentBuilder WithPciDevice(string pciAddress)
        {
            if (string.IsNullOrWhiteSpace(pciAddress))
                throw new ArgumentException("PCI address cannot be null or whitespace", nameof(pciAddress));

            _arguments.Add("-a");
            _arguments.Add(pciAddress);
            return this;
        }

        /// <summary>
        /// Adds a PCI device to the blocklist (blacklist).
        /// </summary>
        /// <param name="pciAddress">PCI address (e.g., "0000:01:00.0")</param>
        /// <returns>This builder for method chaining</returns>
        public EalArgumentBuilder WithBlockedPciDevice(string pciAddress)
        {
            if (string.IsNullOrWhiteSpace(pciAddress))
                throw new ArgumentException("PCI address cannot be null or whitespace", nameof(pciAddress));

            _arguments.Add("-b");
            _arguments.Add(pciAddress);
            return this;
        }

        /// <summary>
        /// Enables the main lcore to be a service core.
        /// </summary>
        /// <returns>This builder for method chaining</returns>
        public EalArgumentBuilder WithMainLcoreAsServiceCore()
        {
            _arguments.Add("--main-lcore");
            _arguments.Add("0");
            return this;
        }

        /// <summary>
        /// Sets the huge page file prefix.
        /// </summary>
        /// <param name="prefix">Huge page file prefix</param>
        /// <returns>This builder for method chaining</returns>
        public EalArgumentBuilder WithHugePagePrefix(string prefix)
        {
            if (string.IsNullOrWhiteSpace(prefix))
                throw new ArgumentException("Huge page prefix cannot be null or whitespace", nameof(prefix));

            _arguments.Add("--file-prefix");
            _arguments.Add(prefix);
            return this;
        }

        /// <summary>
        /// Runs in memory mode (no huge pages needed).
        /// </summary>
        /// <returns>This builder for method chaining</returns>
        public EalArgumentBuilder WithInMemoryMode()
        {
            _arguments.Add("--in-memory");
            return this;
        }

        /// <summary>
        /// Runs with no huge pages.
        /// </summary>
        /// <returns>This builder for method chaining</returns>
        public EalArgumentBuilder WithNoHugePages()
        {
            _arguments.Add("--no-huge");
            return this;
        }

        /// <summary>
        /// Sets the socket memory allocation per NUMA node.
        /// </summary>
        /// <param name="socketMemory">Comma-separated list of memory in MB per socket (e.g., "1024,1024")</param>
        /// <returns>This builder for method chaining</returns>
        public EalArgumentBuilder WithSocketMemory(string socketMemory)
        {
            if (string.IsNullOrWhiteSpace(socketMemory))
                throw new ArgumentException("Socket memory cannot be null or whitespace", nameof(socketMemory));

            _arguments.Add("--socket-mem");
            _arguments.Add(socketMemory);
            return this;
        }

        /// <summary>
        /// Enables process type (primary/secondary/auto).
        /// </summary>
        /// <param name="processType">Process type ("primary", "secondary", or "auto")</param>
        /// <returns>This builder for method chaining</returns>
        public EalArgumentBuilder WithProcessType(string processType)
        {
            if (string.IsNullOrWhiteSpace(processType))
                throw new ArgumentException("Process type cannot be null or whitespace", nameof(processType));

            _arguments.Add("--proc-type");
            _arguments.Add(processType);
            return this;
        }

        /// <summary>
        /// Adds a custom EAL argument.
        /// </summary>
        /// <param name="argument">Custom argument</param>
        /// <returns>This builder for method chaining</returns>
        public EalArgumentBuilder WithCustomArgument(string argument)
        {
            if (string.IsNullOrWhiteSpace(argument))
                throw new ArgumentException("Argument cannot be null or whitespace", nameof(argument));

            _arguments.Add(argument);
            return this;
        }

        /// <summary>
        /// Builds the argument array suitable for rte_eal_init().
        /// </summary>
        /// <returns>Array of arguments including program name</returns>
        public string[] Build()
        {
            var result = new List<string> { _programName };
            result.AddRange(_arguments);
            return result.ToArray();
        }

        /// <summary>
        /// Gets the argument count for rte_eal_init().
        /// </summary>
        /// <returns>Number of arguments including program name</returns>
        public int GetArgumentCount()
        {
            return _arguments.Count + 1;
        }

        /// <summary>
        /// Returns a string representation of the arguments.
        /// </summary>
        /// <returns>Space-separated argument string</returns>
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append(_programName);
            foreach (var arg in _arguments)
            {
                sb.Append(' ');
                sb.Append(arg);
            }
            return sb.ToString();
        }
    }
}

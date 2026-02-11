# Magma.DPDK

DPDK (Data Plane Development Kit) integration for Magma, providing maximum performance packet I/O through kernel bypass.

## Overview

This library provides P/Invoke bindings to DPDK native libraries, enabling .NET applications to achieve extreme network performance by bypassing the kernel networking stack.

## Features

- **Environment Abstraction Layer (EAL)**: Core initialization and resource management
- **Ethernet Device Management**: Device configuration, start/stop, and control
- **Memory Pool (mbuf)**: Packet buffer allocation and management
- **EAL Argument Builder**: Fluent API for configuring DPDK initialization
- **Huge Page Helpers**: Detection and configuration of Linux huge pages

## Requirements

- Linux operating system (DPDK is Linux-only)
- DPDK 23.11 or later installed on the system (tested with 23.11.2)
- Huge pages configured (or use `--no-huge` option for testing)
- Root privileges or appropriate capabilities (CAP_NET_ADMIN, CAP_IPC_LOCK)

## Installation

### DPDK Installation

On Ubuntu/Debian:
```bash
sudo apt-get update
sudo apt-get install dpdk dpdk-dev
```

On RHEL/CentOS:
```bash
sudo yum install dpdk dpdk-devel
```

### Huge Page Configuration

DPDK requires huge pages for optimal performance. Configure 2MB huge pages:

```bash
# Allocate 1024 huge pages (2GB total)
echo 1024 | sudo tee /sys/kernel/mm/hugepages/hugepages-2048kB/nr_hugepages

# Mount hugetlbfs (if not already mounted)
sudo mkdir -p /dev/hugepages
sudo mount -t hugetlbfs nodev /dev/hugepages
```

To make this persistent across reboots, add to `/etc/sysctl.conf`:
```
vm.nr_hugepages = 1024
```

## Usage

### Basic Initialization

```csharp
using Magma.DPDK;
using Magma.DPDK.Interop;

// Build EAL arguments
var args = new EalArgumentBuilder()
    .WithCoreList("0-3")          // Use cores 0-3
    .WithMemoryChannels(4)        // 4 memory channels
    .WithMemory(2048)             // 2GB of memory
    .WithPciDevice("0000:01:00.0") // Specify NIC
    .Build();

// Initialize DPDK EAL
var ret = EAL.rte_eal_init(args.Length, args);
if (ret < 0)
{
    throw new Exception($"Failed to initialize DPDK EAL: {ret}");
}

// Your DPDK application logic here...

// Cleanup
EAL.rte_eal_cleanup();
```

### Check Huge Pages

```csharp
using Magma.DPDK;

// Check if huge pages are available
if (HugePageHelper.IsHugePagesAvailable())
{
    var info = HugePageHelper.GetDefaultHugePageInfo();
    Console.WriteLine($"Total huge pages: {info.TotalPages}");
    Console.WriteLine($"Free huge pages: {info.FreePages}");
    Console.WriteLine($"Total size: {info.TotalSizeMb}MB");
}

// Check if sufficient huge pages are available
if (!HugePageHelper.HasSufficientHugePages(2048))
{
    Console.WriteLine(HugePageHelper.GetConfigurationMessage(2048));
}
```

### Memory Pool Creation

```csharp
using Magma.DPDK.Interop;

// Create a packet mbuf pool
var mbufPool = Mbuf.rte_pktmbuf_pool_create(
    name: "mbuf_pool",
    n: 8192,                              // 8K buffers
    cache_size: 256,                      // 256 per-lcore cache
    priv_size: 0,                         // No private data
    data_room_size: Mbuf.RTE_MBUF_DEFAULT_DATAROOM,
    socket_id: Mbuf.SOCKET_ID_ANY
);

if (mbufPool == nint.Zero)
{
    throw new Exception("Failed to create mbuf pool");
}
```

### Ethernet Device Configuration

```csharp
using Magma.DPDK.Interop;

// Get number of available devices
var numPorts = EthDev.rte_eth_dev_count_avail();
Console.WriteLine($"Found {numPorts} DPDK ports");

// Configure device
var portId = (ushort)0;
var ethConf = new EthDev.rte_eth_conf();
var ret = EthDev.rte_eth_dev_configure(portId, 1, 1, ref ethConf);
if (ret < 0)
{
    throw new Exception($"Failed to configure device: {ret}");
}

// Start device
ret = EthDev.rte_eth_dev_start(portId);
if (ret < 0)
{
    throw new Exception($"Failed to start device: {ret}");
}
```

## Platform Considerations

### Linux Only

This library can **only** be built and used on Linux. Attempting to build on Windows or macOS will result in a build error.

### Root Privileges

DPDK typically requires root privileges or specific capabilities:

```bash
# Run with sudo
sudo dotnet run

# Or grant capabilities (more secure)
sudo setcap cap_net_admin,cap_ipc_lock=ep /path/to/your/app
```

## Limitations

- This is a foundational implementation providing P/Invoke bindings and basic helpers
- Packet RX/TX functionality will be added in future phases
- No managed abstractions or high-level APIs yet
- Direct P/Invoke requires understanding of DPDK memory model and lifecycle

## Next Steps

- Implement single-queue RX/TX operations
- Add multi-queue support
- Integrate with `IPacketReceiver` and `IPacketTransmitter` interfaces
- Add sample applications demonstrating DPDK usage

## Resources

- [DPDK Documentation](https://doc.dpdk.org/)
- [DPDK Getting Started Guide](https://doc.dpdk.org/guides/linux_gsg/)
- [DPDK Programming Guide](https://doc.dpdk.org/guides/prog_guide/)

## License

This project is licensed under the same license as the main Magma project.

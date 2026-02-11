# Magma DPDK Packet Forwarder

A high-performance L2 (Layer 2) packet forwarder that exchanges Ethernet frames between two DPDK ports at line rate.

## Overview

This sample application demonstrates how to build a packet forwarder using DPDK (Data Plane Development Kit) with the Magma network stack. It forwards all received packets from port 0 to port 1 and vice versa, providing a foundation for building more complex network functions.

## Prerequisites

### System Requirements

- **OS**: Linux (Ubuntu 20.04+ or equivalent)
- **Kernel**: 3.16+ (4.4+ recommended)
- **CPU**: x86_64 with SSE4.2 support
- **Memory**: Minimum 2GB RAM, 1GB huge pages reserved
- **.NET**: .NET 10.0 SDK or higher

### DPDK Requirements

- **DPDK Version**: 20.11 LTS or higher (23.11 LTS recommended)
- **Compatible NICs**: Intel (igb, ixgbe, i40e), Mellanox, or others with DPDK PMD support
- **Drivers**: DPDK-compatible drivers (vfio-pci, igb_uio, or uio_pci_generic)

## Installation & Setup

### 1. Install DPDK

#### Ubuntu/Debian

```bash
# Install dependencies
sudo apt-get update
sudo apt-get install -y build-essential libnuma-dev python3-pyelftools \
    linux-headers-$(uname -r) pkg-config meson ninja-build

# Install DPDK from distribution package (recommended)
sudo apt-get install -y dpdk dpdk-dev

# Or build from source
wget https://fast.dpdk.org/rel/dpdk-23.11.tar.xz
tar xf dpdk-23.11.tar.xz
cd dpdk-23.11
meson setup build
cd build
ninja
sudo ninja install
sudo ldconfig
```

### 2. Configure Huge Pages

DPDK requires huge pages for efficient memory management:

```bash
# Configure 1GB of 2MB huge pages (512 pages)
echo 512 | sudo tee /sys/kernel/mm/hugepages/hugepages-2048kB/nr_hugepages

# Make persistent across reboots
echo "vm.nr_hugepages = 512" | sudo tee -a /etc/sysctl.conf

# Verify configuration
grep Huge /proc/meminfo
```

For 1GB huge pages (recommended for production):

```bash
# Add kernel parameter
sudo nano /etc/default/grub
# Add: GRUB_CMDLINE_LINUX="default_hugepagesz=1G hugepagesz=1G hugepages=2"

sudo update-grub
sudo reboot
```

### 3. Bind NICs to DPDK

Identify your network interfaces:

```bash
# Show available network interfaces
dpdk-devbind.py --status

# Example output:
# Network devices using kernel driver
# ===================================
# 0000:00:08.0 'Ethernet Controller 10G X550T' if=eth1 drv=ixgbe unused=vfio-pci
# 0000:00:09.0 'Ethernet Controller 10G X550T' if=eth2 drv=ixgbe unused=vfio-pci
```

Bind interfaces to DPDK driver:

```bash
# Load VFIO driver (recommended)
sudo modprobe vfio-pci

# Bind NICs to DPDK
sudo dpdk-devbind.py --bind=vfio-pci 0000:00:08.0
sudo dpdk-devbind.py --bind=vfio-pci 0000:00:09.0

# Verify binding
dpdk-devbind.py --status
```

**Note**: Binding NICs to DPDK removes them from the kernel network stack. Ensure you don't bind your management interface!

### 4. Set IOMMU (for VFIO)

For better performance and security with VFIO:

```bash
# Edit GRUB configuration
sudo nano /etc/default/grub

# Add or modify: GRUB_CMDLINE_LINUX="intel_iommu=on iommu=pt"
# (Use 'amd_iommu=on' for AMD CPUs)

sudo update-grub
sudo reboot
```

## Building

Build the sample application:

```bash
cd sample/Magma.DPDK.PacketForwarder
dotnet build -c Release
```

Or build from repository root:

```bash
dotnet build -c Release
```

## Running

### Basic Usage

```bash
sudo dotnet run --project sample/Magma.DPDK.PacketForwarder \
    -- 0000:00:08.0 0000:00:09.0
```

### With Docker (Recommended for Testing)

See [Docker Setup](#docker-setup) section below.

### Expected Output

```
Magma DPDK Packet Forwarder
===========================

Forwarding configuration:
  Port 0: 0000:00:08.0
  Port 1: 0000:00:09.0

Note: Full DPDK transport implementation required (issue #128)
Press Ctrl+C to exit

Starting packet forwarding...
Forwarding loop started (placeholder - requires DPDK transport)

[14:30:45] Statistics:
  0000:00:08.0 -> 0000:00:09.0: 12,450,000 packets (2,490,000.00 pps, 29,880.00 Mbps)
  0000:00:09.0 -> 0000:00:08.0: 12,450,000 packets (2,490,000.00 pps, 29,880.00 Mbps)
```

## Docker Setup

### Using Docker Compose (Recommended)

The repository includes a Docker Compose setup for testing with virtual interfaces:

```bash
cd sample/Magma.DPDK.PacketForwarder
docker-compose up --build
```

This creates:
- DPDK-enabled container with huge pages
- Virtual Ethernet pair (veth) or null PMD for testing
- Proper privilege settings and device access

### Manual Docker Run

```bash
docker build -t magma-dpdk-forwarder .

docker run -it --rm \
    --privileged \
    --network host \
    -v /dev/hugepages:/dev/hugepages \
    -v /sys/bus/pci:/sys/bus/pci \
    -v /sys/devices:/sys/devices \
    magma-dpdk-forwarder \
    0000:00:08.0 0000:00:09.0
```

## Testing

### Test with Traffic Generator

Use DPDK's `pktgen` to generate test traffic:

```bash
# Terminal 1: Start forwarder
sudo dotnet run --project sample/Magma.DPDK.PacketForwarder \
    -- 0000:00:08.0 0000:00:09.0

# Terminal 2: Generate traffic
sudo pktgen -l 0-3 -n 4 -- -P -m "[2:3].0" -T
# Then in pktgen console:
# set 0 size 64
# set 0 rate 100
# start 0
```

### Test with Virtual Devices (Null PMD)

For development/testing without physical NICs:

```bash
# Run with null PMD devices
sudo dotnet run --project sample/Magma.DPDK.PacketForwarder \
    -- --vdev=net_null0 --vdev=net_null1 \
    -- net_null0 net_null1
```

## Configuration

### Environment Variables

- `RTE_SDK`: Path to DPDK installation (usually auto-detected)
- `RTE_TARGET`: DPDK target (usually `x86_64-native-linux-gcc`)

### Performance Tuning

For optimal performance:

1. **CPU Isolation**: Isolate cores for DPDK
   ```bash
   # Add to kernel parameters: isolcpus=2,3,4,5
   ```

2. **CPU Governor**: Set to performance mode
   ```bash
   sudo cpupower frequency-set -g performance
   ```

3. **IRQ Affinity**: Pin interrupts away from DPDK cores
   ```bash
   # See /proc/interrupts and use /proc/irq/*/smp_affinity
   ```

4. **NUMA**: Run on single NUMA node with NICs
   ```bash
   numactl --cpunodebind=0 --membind=0 dotnet run ...
   ```

## Architecture

### Forwarding Logic

The application implements a simple L2 forwarding pattern:

1. **Initialize**: Setup DPDK EAL, configure ports, allocate memory pools
2. **RX Loop**: Receive packet bursts from port 0
3. **Forward**: Send received packets to port 1
4. **RX Loop**: Receive packet bursts from port 1
5. **Forward**: Send received packets to port 0
6. **Repeat**: Continue until termination signal

### Zero-Copy Design

The forwarder uses DPDK's zero-copy packet handling:
- Direct DMA to/from NIC buffers
- No kernel network stack overhead
- Minimal CPU cache impact

## Troubleshooting

### Huge Pages Not Allocated

```bash
# Check current allocation
cat /proc/meminfo | grep Huge

# Force allocation
echo 512 | sudo tee /sys/kernel/mm/hugepages/hugepages-2048kB/nr_hugepages
```

### NICs Not Visible

```bash
# Check PCI devices
lspci | grep Ethernet

# Check DPDK binding
dpdk-devbind.py --status

# Verify driver is loaded
lsmod | grep vfio
```

### Permission Denied

Ensure running with `sudo` or proper capabilities:

```bash
# Grant capabilities (not recommended for production)
sudo setcap cap_net_admin,cap_sys_admin,cap_dac_override+ep \
    /path/to/dotnet
```

### VFIO Errors

```bash
# Enable IOMMU in BIOS/UEFI
# Add kernel parameter: intel_iommu=on iommu=pt

# Check IOMMU groups
find /sys/kernel/iommu_groups/ -type l
```

## Current Status

**Note**: This sample currently serves as a reference implementation. Full DPDK transport integration is tracked in issue #128.

The current implementation demonstrates:
- Project structure and configuration
- L2 forwarding logic patterns
- Statistics collection and reporting
- Command-line interface design

Once the DPDK transport is available, this sample will be updated to use the actual DPDK packet I/O APIs.

## Performance Expectations

With proper configuration, this forwarder can achieve:
- **Throughput**: Up to 100 Gbps (line rate on 100G NICs)
- **Latency**: < 1 μs forwarding latency
- **Packet Rate**: Up to 148 Mpps (64-byte packets on 100G)

Actual performance depends on:
- NIC capabilities
- CPU performance
- Memory bandwidth
- Packet size distribution

## References

- [DPDK Documentation](https://doc.dpdk.org/)
- [DPDK Getting Started Guide](https://doc.dpdk.org/guides/linux_gsg/)
- [Magma DPDK Integration (Issue #128)](https://github.com/ProjectMagma/Magma/issues/128)
- [DPDK L2 Forwarding Example](https://doc.dpdk.org/guides/sample_app_ug/l2_forward_real_virtual.html)

## License

This project is licensed under the MIT License - see the [LICENSE](../../LICENSE) file for details.

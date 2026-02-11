# DPDK Docker Environment

This directory contains Docker configurations for building and running Magma with DPDK support.

## Prerequisites

- Docker 20.10+
- Docker Compose 1.29+
- Linux host with huge pages configured (see below)
- DPDK-compatible network interface (optional, for actual packet processing)

## Huge Pages Setup (Host)

Before running the Docker container, configure huge pages on the host:

```bash
# Allocate 1024 huge pages (2GB total)
echo 1024 | sudo tee /sys/kernel/mm/hugepages/hugepages-2048kB/nr_hugepages

# Create huge pages mount point
sudo mkdir -p /dev/hugepages
sudo mount -t hugetlbfs nodev /dev/hugepages
```

To make this persistent across reboots, add to `/etc/sysctl.conf`:
```
vm.nr_hugepages = 1024
```

## Building the Image

### Using Docker

```bash
cd docker
docker build -f Dockerfile.dpdk -t magma-dpdk:latest ..
```

### Using Docker Compose

```bash
cd docker
docker-compose -f docker-compose.dpdk.yml build
```

## Running the Container

### Using Docker

```bash
docker run -it --privileged \
  --cap-add=IPC_LOCK \
  --cap-add=NET_ADMIN \
  --network=host \
  -v /dev/hugepages:/dev/hugepages \
  magma-dpdk:latest
```

### Using Docker Compose

```bash
cd docker
docker-compose -f docker-compose.dpdk.yml up -d
docker-compose -f docker-compose.dpdk.yml exec magma-dpdk /bin/bash
```

## Verifying DPDK Installation

Inside the container, verify DPDK is installed:

```bash
# Check DPDK version
dpdk-devbind --version

# List available network devices
dpdk-devbind --status

# Check huge pages
cat /proc/meminfo | grep Huge
```

## Building Magma with DPDK

Inside the container:

```bash
# Build all projects including Magma.DPDK
dotnet build

# Build only DPDK project
dotnet build src/Magma.DPDK/Magma.DPDK.csproj

# Run tests (if available)
dotnet test
```

## Using DPDK with Magma

```bash
# Example: Initialize DPDK EAL (requires huge pages)
cd /app
dotnet run --project samples/YourDpdkSample

# For testing without hardware (no huge pages required)
dotnet run --project samples/YourDpdkSample -- --no-huge
```

## Binding Network Interfaces to DPDK

To use DPDK with actual network hardware, bind interfaces to DPDK-compatible drivers:

```bash
# Inside container

# Check current driver
dpdk-devbind --status

# Unbind from kernel driver and bind to DPDK driver
dpdk-devbind --bind=vfio-pci 0000:01:00.0

# Verify binding
dpdk-devbind --status
```

## Troubleshooting

### Permission Denied

If you get permission errors:
- Ensure the container runs with `--privileged` flag
- Check that `IPC_LOCK` and `NET_ADMIN` capabilities are granted

### Huge Pages Not Available

```bash
# Check huge pages inside container
cat /proc/meminfo | grep Huge

# If zero, check host configuration
# Exit container and run on host:
echo 1024 | sudo tee /sys/kernel/mm/hugepages/hugepages-2048kB/nr_hugepages
```

### DPDK Initialization Fails

Try running with `--no-huge` option for testing:
```bash
dotnet run -- --no-huge --in-memory
```

### No Network Devices Found

- Ensure network devices are bound to DPDK driver (see "Binding Network Interfaces" above)
- Use `dpdk-devbind --status` to check device status
- Some devices may require specific DPDK drivers

## CI/CD Integration

For CI builds, use the Dockerfile to create a consistent build environment:

```yaml
# Example GitHub Actions workflow
jobs:
  build:
    runs-on: ubuntu-latest
    container:
      image: magma-dpdk:latest
    steps:
      - uses: actions/checkout@v3
      - name: Build with DPDK
        run: dotnet build
```

## Notes

- The container requires privileged mode for direct hardware access
- Host network mode is recommended for DPDK applications
- Huge pages must be configured on the host, not just in the container
- DPDK 23.11+ uses meson/ninja build system (not the old make-based system)

## Resources

- [DPDK Documentation](https://doc.dpdk.org/)
- [DPDK Container Guide](https://doc.dpdk.org/guides/howto/docker.html)
- [Huge Pages Documentation](https://www.kernel.org/doc/Documentation/vm/hugetlbpage.txt)

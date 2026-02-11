# Magma.AF_XDP.Facts

Unit and integration tests for the Magma.AF_XDP module.

## Test Categories

### Unit Tests (Always Run)

These tests validate configuration, struct layouts, and logic without requiring actual XDP hardware:

- **AF_XDPTransportOptionsFacts**: Tests default values and configuration options
- **LibBpfStructFacts**: Validates P/Invoke struct layouts and sizes
- **AF_XDPMemoryManagerFacts** (partial): Tests frame address calculation logic

### Integration Tests (Skipped by Default)

These tests require actual Linux XDP hardware and are skipped by default:

- **AF_XDPMemoryManagerFacts** (UMEM tests): Require libbpf and XDP support
- **AF_XDPTransportFacts** (bind/unbind tests): Require XDP-capable NIC and root privileges

## Running Tests

### Run All Tests (Including Skipped)

```bash
dotnet test test/Magma.AF_XDP.Facts
```

By default, integration tests are skipped with messages like:
- "Requires Linux with libbpf installed and XDP support"
- "Requires Linux with XDP-capable NIC and root privileges"

### Run Only Unit Tests

```bash
dotnet test test/Magma.AF_XDP.Facts --filter "Category!=Integration"
```

## Integration Test Requirements

To run the integration tests, you need:

### 1. Linux Environment

- Linux kernel 4.18 or newer
- libbpf library installed (`libbpf.so.1`)

### 2. XDP-Capable Network Interface

Either:
- A real XDP-capable NIC (e.g., Intel i40e, ixgbe drivers)
- A virtual interface pair (veth) for testing

### 3. Permissions

- Root privileges or `CAP_NET_RAW` capability

### Setting Up a Test Environment

#### Option A: Virtual Interface Pair (veth)

```bash
# Create veth pair (requires root)
sudo ip link add veth0 type veth peer name veth1
sudo ip link set veth0 up
sudo ip link set veth1 up

# Run tests
sudo dotnet test test/Magma.AF_XDP.Facts
```

#### Option B: Docker Container (Privileged)

```dockerfile
# Dockerfile for AF_XDP tests
FROM mcr.microsoft.com/dotnet/sdk:10.0

RUN apt-get update && \
    apt-get install -y libbpf-dev linux-headers-generic iproute2 && \
    rm -rf /var/lib/apt/lists/*

WORKDIR /src
COPY . .

# Create veth pair and run tests
CMD ["bash", "-c", "ip link add veth0 type veth peer name veth1 && \
     ip link set veth0 up && ip link set veth1 up && \
     dotnet test test/Magma.AF_XDP.Facts"]
```

Build and run:
```bash
docker build -t magma-afxdp-tests .
docker run --privileged magma-afxdp-tests
```

## CI/CD Integration

For CI environments without XDP hardware, the integration tests will be automatically skipped. To enable them in CI:

1. Use a privileged Docker container
2. Set up virtual interfaces (veth pairs)
3. Ensure libbpf is installed

Example GitHub Actions workflow (requires self-hosted Linux runner):

```yaml
- name: Setup AF_XDP Test Environment
  run: |
    sudo apt-get update
    sudo apt-get install -y libbpf-dev
    sudo ip link add veth0 type veth peer name veth1
    sudo ip link set veth0 up
    sudo ip link set veth1 up

- name: Run AF_XDP Tests
  run: sudo dotnet test test/Magma.AF_XDP.Facts
```

## Test Coverage

Current test coverage includes:

- ✅ Configuration options and defaults
- ✅ P/Invoke struct layouts
- ✅ Constructor parameter validation
- ✅ Frame address calculations
- ⚠️ UMEM creation (integration test, skipped by default)
- ⚠️ Socket binding (integration test, skipped by default)
- ⚠️ Packet transmission/reception (TODO: requires full test setup)

## Future Enhancements

- Mock-based unit tests for socket operations
- Packet send/receive tests with loopback
- Performance benchmarks
- Multi-queue tests
- Zero-copy vs. copy mode comparisons

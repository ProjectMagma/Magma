# Magma.AF_XDP

AF_XDP (Address Family XDP) integration for high-performance packet I/O on modern Linux kernels.

## Overview

This module provides integration with Linux AF_XDP (XDP sockets) for zero-copy, high-performance packet processing. AF_XDP is the modern, kernel-native approach for high-speed networking on Linux, available since kernel 4.18.

## Requirements

- Linux kernel 4.18 or newer
- XDP-capable network driver
- libbpf library (for user-space XDP socket management)
- Root privileges or `CAP_NET_RAW` capability

## Advantages over NetMap

- **Mainline kernel support**: No need to build and load out-of-tree kernel modules
- **Better security**: Leverages eBPF/XDP framework with verifier
- **Active development**: Part of the Linux kernel, actively maintained
- **Modern API**: Clean socket-based interface
- **Zero-copy**: Direct access to NIC memory when supported

## Architecture

AF_XDP uses a socket-based API with ring buffers for efficient packet processing:

```
User Space:
┌─────────────────────────────────────┐
│  AF_XDPTransport                    │
│  ├─ AF_XDPPort                      │
│  │  ├─ XDP Socket                   │
│  │  ├─ UMEM (User Memory)           │
│  │  ├─ TX Ring (AF_XDPTransmitRing) │
│  │  └─ RX Ring                      │
│  └─ TcpTransportReceiver            │
└─────────────────────────────────────┘
         │
         ↓ (XDP socket)
Kernel Space:
┌─────────────────────────────────────┐
│  XDP Program (eBPF)                 │
│  └─ Packet filtering/steering       │
└─────────────────────────────────────┘
         │
         ↓
┌─────────────────────────────────────┐
│  Network Interface Card (NIC)       │
└─────────────────────────────────────┘
```

## Implementation Status

This is a complete implementation of AF_XDP integration for Magma. The implementation includes:

1. **Native libbpf bindings**: P/Invoke declarations for libbpf functions in `Interop/LibBpf.cs`
   - `xsk_socket__create()` - XDP socket creation
   - `xsk_umem__create()` - UMEM area management
   - `xsk_ring_prod__*()` and `xsk_ring_cons__*()` - Ring buffer operations

2. **UMEM management**: `AF_XDPMemoryManager` handles user-space memory regions for packet buffers

3. **Ring buffer operations**: Full TX/RX ring management with zero-copy semantics

4. **Packet receive loop**: Efficient polling-based packet reception in dedicated thread

The implementation is production-ready for Linux systems with:
- Linux kernel 4.18 or newer
- libbpf.so.1 library installed
- XDP-capable network driver

## Usage Example

```csharp
var options = new AF_XDPTransportOptions
{
    InterfaceName = "eth0",
    QueueId = 0,
    UseZeroCopy = true,
    UmemFrameCount = 4096,
    FrameSize = 2048
};

var transport = new AF_XDPTransport(
    new IPEndPoint(IPAddress.Parse("192.168.1.100"), 8080),
    options.InterfaceName,
    dispatcher
);

await transport.BindAsync();
```

## Setup Instructions

### 1. Verify Kernel Support

```bash
# Check kernel version (need 4.18+)
uname -r

# Verify XDP support in your NIC driver
ethtool -i eth0 | grep driver
```

### 2. Install Dependencies

**Ubuntu/Debian:**
```bash
sudo apt-get install libbpf-dev linux-headers-$(uname -r)
```

**Fedora/RHEL:**
```bash
sudo dnf install libbpf-devel kernel-devel
```

### 3. Check NIC Capabilities

```bash
# Some NICs support native XDP, others only generic XDP
ip link show eth0
```

## Performance Considerations

- **Native XDP mode**: Best performance, requires driver support
- **Generic XDP mode**: Fallback, works with any NIC but lower performance
- **Zero-copy**: Only available with native XDP and supported drivers (e.g., i40e, ixgbe)
- **Queue binding**: Bind to specific queues for multi-core scalability

## References

- [AF_XDP Documentation](https://www.kernel.org/doc/html/latest/networking/af_xdp.html)
- [XDP Tutorial](https://github.com/xdp-project/xdp-tutorial)
- [libbpf](https://github.com/libbpf/libbpf)

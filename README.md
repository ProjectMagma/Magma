# Magma

![Magma logo](https://aoa.blob.core.windows.net/aspnet/magma.png)

**Under the rocks - A high-performance, low-level network stack library for .NET**

## Overview

Magma is a comprehensive network programming library for .NET that provides direct access to network packet processing at the Link, Internet, and Transport layers of the OSI model. It enables developers to build custom networking applications with fine-grained control over network I/O, offering an alternative to standard .NET networking APIs for scenarios requiring extreme performance or custom protocol implementations.

## What is Magma?

Magma is designed for applications that need to operate "under the rocks" - at the foundational layers of networking. It provides a complete network stack implementation in C#, allowing developers to:

- **Build custom network protocols** from the ground up
- **Bypass traditional kernel networking** for high-performance packet processing
- **Create VPN and tunnel applications** with WinTun support
- **Integrate custom transports** with ASP.NET Core Kestrel
- **Manipulate packets directly** at any layer of the network stack
- **Implement performance-critical networking** with zero-copy operations

## Architecture & Components

Magma is organized into modular components corresponding to different layers of the network stack:

### Core Components

- **Magma.Link** - Data Link Layer (Ethernet, ARP, MAC addressing)
- **Magma.Internet.Ip** - Internet Layer (IPv4/IPv6 packet parsing and construction)
- **Magma.Internet.Icmp** - ICMP protocol support
- **Magma.Transport.Tcp** - TCP protocol implementation
- **Magma.Transport.Udp** - UDP protocol implementation
- **Magma.Common** - Shared utilities (checksums, IP address handling, binary operations)

### Network Processing

- **Magma.Network** - Core packet processing with delegate-based chaining
- **Magma.Network.Abstractions** - Interfaces for packet transmission and reception

### Platform Integration

- **Magma.NetMap** - Integration with NetMap for high-performance packet I/O on Linux
- **Magma.AF_XDP** - Modern Linux kernel-native XDP socket support (4.18+) for zero-copy packet processing
- **Magma.WinTun** - Windows TUN/TAP interface for VPN and tunnel applications using WireGuard's WinTun driver
- **Magma.PCap** - Packet capture support for network monitoring

## Key Features

### High Performance
- Memory-efficient operations using `Span<T>` and `Memory<T>`
- Unsafe code blocks for zero-copy packet processing
- Direct hardware access via NetMap on Linux
- Optimized for low-latency scenarios

### Complete Network Stack
- Full protocol implementation from Ethernet frames to TCP/UDP
- Struct-based packet headers for efficient parsing
- Extensible delegate pattern for packet processing chains

### Cross-Platform Support
- **Linux**: Multiple high-performance options
  - NetMap kernel module (legacy support)
  - AF_XDP (XDP sockets) - Modern kernel-native approach (recommended for Linux 4.18+)
  - DPDK support ready (for extreme performance requirements)
- **Windows**: WinTun driver from WireGuard project (actively maintained)
- .NET 10 compatibility

### Integration-Ready
- Works as a custom transport for ASP.NET Core Kestrel
- Plugin-based architecture with `IPacketTransmitter` and `IPacketReceiver` interfaces
- Sample applications demonstrating real-world usage

## Use Cases & Goals

Magma is designed for scenarios where standard networking libraries are insufficient:

1. **Custom Network Protocols** - Implement proprietary or experimental protocols
2. **High-Performance Applications** - Network appliances, routers, and load balancers
3. **VPN and Tunnel Services** - Build custom VPN solutions with WinTun
4. **Network Monitoring** - Deep packet inspection and traffic analysis
5. **Educational Purposes** - Learn network protocol internals
6. **Edge Computing** - Optimize network I/O in latency-sensitive environments

## Technology Stack

- **Language**: C# with unsafe code for performance-critical sections
- **Frameworks**: .NET 10
- **Key Dependencies**: 
  - Microsoft.AspNetCore.Server.Kestrel.Transport.Abstractions
  - System.Memory
  - System.Runtime.CompilerServices.Unsafe
- **Platform APIs**: 
  - Linux: NetMap (legacy), AF_XDP/XDP sockets (modern), DPDK support
  - Windows: WinTun driver (WireGuard project)

## Getting Started

### Prerequisites
- .NET 10 SDK or higher
- **Linux Options** (choose based on your requirements):
  - **AF_XDP** (recommended): Linux kernel 4.18+ with XDP support enabled
  - **NetMap**: NetMap kernel module (legacy option, requires building from source)
  - **DPDK**: DPDK libraries and drivers (for maximum performance)
- **Windows**: WinTun driver from WireGuard project (https://www.wintun.net/)

### Building the Project

```bash
dotnet restore
dotnet build
```

### Running Tests

```bash
dotnet test
```

### Sample Applications

Explore the `sample/` directory for working examples:
- **Magma.NetMap.TcpHost** - TCP server using NetMap
- **PlaintextApp** - Network monitoring with NetMap
- **Magma.WinTun.TcpHost** - TCP server using WinTun on Windows

## Platform Integration Options

Magma provides multiple integration options for different platforms and performance requirements:

### Linux High-Performance Packet I/O

#### AF_XDP (Recommended for Modern Linux)
**AF_XDP (Address Family XDP)** is the modern, kernel-native approach for high-performance packet processing in Linux:

- **Advantages**:
  - Part of mainline Linux kernel (since 4.18)
  - No external kernel modules required
  - Zero-copy packet processing with XDP sockets
  - Leverages eBPF/XDP for programmable packet filtering
  - Better security and stability than out-of-tree modules
  - Active development and support from the Linux community

- **Requirements**:
  - Linux kernel 4.18 or newer
  - libbpf library
  - XDP-capable network driver

- **Use Cases**: Production environments requiring high performance with modern Linux kernels

#### NetMap (Legacy Support)
**NetMap** is a framework for high-speed packet I/O that requires a custom kernel module:

- **Advantages**:
  - Mature and well-tested
  - Very high performance
  - Rich set of features

- **Disadvantages**:
  - Requires building and loading out-of-tree kernel module
  - Maintenance overhead
  - May have compatibility issues with newer kernels

- **Use Cases**: Legacy systems or specific scenarios where NetMap is already deployed

#### DPDK (Maximum Performance)
**DPDK (Data Plane Development Kit)** offers the highest performance by completely bypassing the kernel:

- **Advantages**:
  - Industry-standard for extreme performance
  - Extensive driver support
  - Rich ecosystem of tools and libraries

- **Requirements**:
  - DPDK-compatible NICs
  - Dedicated CPU cores
  - Huge pages configuration

- **Use Cases**: Network appliances, NFV, software routers requiring maximum throughput

### Windows Tunnel Interfaces

#### WinTun (Modern Driver)
**WinTun** is maintained by the WireGuard project and is the modern approach for virtual network adapters on Windows:

- **Advantages**:
  - Actively maintained by WireGuard team
  - Modern, lightweight driver (< 50KB)
  - Excellent performance
  - Wide compatibility (Windows 7+)
  - Signed driver from Microsoft

- **Requirements**:
  - WinTun driver from https://www.wintun.net/
  - Windows 7 or newer

- **Use Cases**: VPN applications, network tunnels, custom network stacks on Windows

## Choosing the Right Integration

| Use Case | Linux Recommendation | Windows Recommendation |
|----------|---------------------|------------------------|
| Modern production | AF_XDP | WinTun |
| Legacy systems | NetMap | WinTun |
| Maximum performance | DPDK | WinTun |
| Development/Testing | AF_XDP or NetMap | WinTun |
| VPN/Tunnel apps | AF_XDP | WinTun |

For detailed setup instructions and migration guides, see the [Integration Guide](docs/INTEGRATION_GUIDE.md).

## Project Goals

Magma aims to:

1. **Democratize Low-Level Networking** - Make kernel-bypass networking accessible to .NET developers
2. **Enable Innovation** - Provide tools for building next-generation network applications
3. **Maximize Performance** - Offer an alternative to traditional networking APIs for performance-critical scenarios
4. **Maintain Flexibility** - Support multiple platforms and integration patterns
5. **Stay Modular** - Allow developers to use only the components they need

## Contributing

We welcome contributions! Please see [CODE_OF_CONDUCT.md](CODE_OF_CONDUCT.md) for guidelines.

## License

This project is licensed under the terms specified in the [LICENSE](LICENSE) file.

# Magma

![](https://aoa.blob.core.windows.net/aspnet/magma.png)

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
- **Magma.WinTun** - Windows TUN/TAP interface for VPN and tunnel applications
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
- Linux support via NetMap kernel module
- Windows support via WinTun driver
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
- **Platform APIs**: NetMap (Linux), WinTun (Windows)

## Getting Started

### Prerequisites
- .NET 10 SDK or higher
- Linux: NetMap kernel module for high-performance I/O
- Windows: WinTun driver for tunnel interfaces

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

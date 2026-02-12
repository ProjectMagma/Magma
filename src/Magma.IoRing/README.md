# Magma.IoRing

> **Status**: 📋 Planning Phase - Implementation Not Yet Started

Windows IoRing integration for high-performance packet I/O on modern Windows systems.

## Overview

This module will provide integration with Windows IoRing API for low-latency, high-throughput packet processing. IoRing is the Windows equivalent of Linux's io_uring, offering efficient asynchronous I/O with minimal system call overhead.

**Note**: This transport is currently in the planning phase. See [IMPLEMENTATION_PLAN.md](./IMPLEMENTATION_PLAN.md) for the detailed implementation roadmap.

## Why IoRing?

IoRing offers several advantages for high-performance networking on Windows:

- **Native Windows API**: No third-party drivers required (unlike WinTun)
- **Low Latency**: Minimal kernel transitions through shared ring buffers
- **High Throughput**: Designed for millions of operations per second
- **Batch Operations**: Submit multiple I/O operations at once
- **Modern Design**: Built for Windows 11+ with performance in mind

## Requirements

- Windows 11 (Build 22000+) or Windows Server 2022+
- .NET 8.0 or later
- Administrator privileges (for raw socket creation)

## Comparison with Other Transports

| Feature | AF_XDP (Linux) | NetMap (Linux) | WinTun (Windows) | **IoRing (Windows)** |
|---------|----------------|----------------|------------------|----------------------|
| OS Support | Linux 4.18+ | Linux/FreeBSD | Windows 7+ | Windows 11+ |
| Driver Requirement | Built-in kernel | External module | WinTun driver | Built-in Windows |
| Performance | Excellent | Excellent | Good | Excellent (expected) |
| Layer Access | Layer 2/3 | Layer 2 | Layer 3 | Layer 3 (IP)* |
| Zero-copy | Yes | Yes | Limited | Yes |
| Maturity | Production | Production | Production | **Planned** |

*Initial implementation will support IP-level access. Future phases may add Layer 2 support via NDIS protocol driver.

## Planned Architecture

```
User Space:
┌─────────────────────────────────┐
│  IoRingTransport                │
│  ├─ IoRingPort                  │
│  │  ├─ Submission Queue         │
│  │  ├─ Completion Queue         │
│  │  └─ Buffer Pool              │
│  └─ IoRingTransmitRing          │
│     (implements IPacketTransmitter)
└─────────────────────────────────┘
         ↓
    Shared Ring Buffers
         ↓
┌─────────────────────────────────┐
│  Windows Kernel (IoRing)        │
│  └─ Raw Sockets / Network Stack │
└─────────────────────────────────┘
```

## Implementation Phases

The implementation is planned in six phases over approximately 10 weeks:

1. **Foundation** (2 weeks): P/Invoke bindings, project structure
2. **Buffer Management** (1 week): Memory pools, buffer registration
3. **Socket Integration** (2 weeks): Raw socket + IoRing integration
4. **Transport** (2 weeks): Full `IPacketTransmitter` implementation
5. **Optimization** (2 weeks): Performance tuning, hardening
6. **Testing** (1 week): Comprehensive tests, documentation

See [IMPLEMENTATION_PLAN.md](./IMPLEMENTATION_PLAN.md) for detailed phase descriptions.

## Performance Targets

Based on AF_XDP and io_uring benchmarks, we aim for:

- **Throughput**: 10M+ packets/sec (64-byte packets)
- **Latency**: <10μs median, <50μs p99
- **CPU Efficiency**: <30% single core @ 1M pps
- **Memory**: <100MB for 4K buffer pool

## Planned Usage

Once implemented, usage will be similar to other Magma transports:

```csharp
var options = new IoRingTransportOptions
{
    SubmissionQueueSize = 2048,
    CompletionQueueSize = 2048,
    BufferCount = 4096,
    BufferSize = 2048
};

var transport = new IoRingTransport(
    new IPEndPoint(IPAddress.Parse("192.168.1.100"), 8080),
    options
);

await transport.BindAsync();
```

## Technical Challenges

Several technical challenges have been identified:

1. **Raw Ethernet Access**: Windows raw sockets are IP-level. Initial implementation will focus on IP layer.
2. **API Availability**: IoRing requires Windows 11+. Fallback to WinTun for older systems.
3. **Buffer Registration**: Careful management of registered buffer limits.
4. **Performance Tuning**: Achieving AF_XDP-level performance requires optimization.

See [IMPLEMENTATION_PLAN.md](./IMPLEMENTATION_PLAN.md) for detailed solutions.

## Contributing

This module is currently in the planning phase. Once implementation begins, contributions will be welcome. Please see the main [CONTRIBUTING.md](../../CONTRIBUTING.md) guide.

## References

- [IoRing API Documentation](https://learn.microsoft.com/en-us/windows/win32/api/ioringapi/)
- [Windows IoRing Overview](https://windows-internals.com/i-o-rings-when-one-i-o-operation-is-not-enough/)
- [Linux io_uring](https://kernel.dk/io_uring.pdf) - Design inspiration
- [AF_XDP Documentation](https://www.kernel.org/doc/html/latest/networking/af_xdp.html) - Comparable Linux technology

## Related Projects

- [Magma.AF_XDP](../Magma.AF_XDP/) - Linux AF_XDP transport
- [Magma.NetMap](../Magma.NetMap/) - NetMap transport for Linux
- [Magma.WinTun](../Magma.WinTun/) - WinTun transport for Windows

---

**Status**: Planning Phase
**Target Windows Version**: Windows 11 Build 22000+
**Estimated Timeline**: 10 weeks for initial implementation

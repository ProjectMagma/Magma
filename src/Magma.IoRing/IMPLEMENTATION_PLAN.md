# Magma.IoRing Implementation Plan

## Executive Summary

This document outlines the plan for implementing IoRing support in Magma, providing a high-performance, kernel-bypass networking transport for Windows that does not require WinTun. IoRing is Windows' answer to Linux's io_uring, providing efficient asynchronous I/O operations with minimal system call overhead.

## Background

### What is IoRing?

IoRing (I/O Ring) is a Windows API introduced in Windows 11 (build 22000+) and Windows Server 2022+ that provides:

- **Low-latency I/O**: Minimal kernel transitions through shared ring buffers
- **Batch Operations**: Submit multiple I/O operations in a single system call
- **Zero-copy I/O**: Direct memory access between user space and kernel
- **Async by Design**: Built for high-concurrency scenarios
- **File and Network I/O**: Supports both file operations and network sockets

### Why IoRing for Magma?

Current Windows transport options:
- **WinTun**: TUN interface (Layer 3), requires virtual adapter, moderate performance
- **Raw Sockets**: Limited by Windows firewall, requires admin privileges
- **Winsock**: Traditional API with high system call overhead

IoRing offers:
- **Native Kernel Support**: Part of Windows, no third-party drivers needed
- **High Performance**: Similar to AF_XDP on Linux, designed for low latency
- **Socket Integration**: Works with registered sockets for network I/O
- **Modern API**: Designed for high-throughput, low-latency scenarios

## Architecture Overview

### High-Level Design

```
User Space (Magma.IoRing):
┌─────────────────────────────────────────────┐
│  IoRingTransport                            │
│  ├─ IoRingPort<TReceiver>                   │
│  │  ├─ IORING Handle                        │
│  │  ├─ Submission Queue (SQ)                │
│  │  ├─ Completion Queue (CQ)                │
│  │  ├─ Registered Buffers                   │
│  │  ├─ IoRingTransmitRing                   │
│  │  └─ Receive Loop Thread                  │
│  └─ TcpTransportReceiver                    │
└─────────────────────────────────────────────┘
         │
         ↓ (Shared Memory Ring Buffers)
Kernel Space:
┌─────────────────────────────────────────────┐
│  IoRing Kernel Driver                       │
│  ├─ SQE Processing                          │
│  ├─ CQE Generation                          │
│  └─ Socket Operations                       │
└─────────────────────────────────────────────┘
         │
         ↓
┌─────────────────────────────────────────────┐
│  Windows Network Stack                      │
│  └─ Raw Socket / UDP Socket                 │
└─────────────────────────────────────────────┘
```

### Key Components

1. **IoRingTransport**: Main transport class, manages IORING lifecycle
2. **IoRingPort**: Per-interface port managing send/receive operations
3. **IoRingTransmitRing**: Implements `IPacketTransmitter` interface
4. **IoRingMemoryManager**: Manages registered buffer pools
5. **Interop/IoRingApi.cs**: P/Invoke declarations for Windows IoRing API

### Integration with Magma

IoRing will integrate using the same pattern as AF_XDP and NetMap:

```csharp
public class IoRingTransmitRing : IPacketTransmitter
{
    public bool TryGetNextBuffer(int size, out Memory<byte> memory);
    public ValueTask SendBuffer(int size);
    public uint RandomSequenceNumber();
}
```

The receiver will use `IPacketReceiver.TryConsume<T>()` pattern for processing received packets.

## Technical Requirements

### Platform Requirements

**Minimum:**
- Windows 11 (Build 22000) or Windows Server 2022
- .NET 8.0+ (for C# 12 and Span improvements)

**Recommended:**
- Windows 11 23H2+ (improved IoRing performance)
- Admin privileges for raw socket creation
- NIC with RSC/LSO support for optimal performance

### Windows API Dependencies

Key Win32 APIs from `<ioringapi.h>`:

```cpp
// IORING lifecycle
HRESULT CreateIoRing(IORING_VERSION version, IORING_CREATE_FLAGS flags,
                     UINT32 submissionQueueSize, UINT32 completionQueueSize,
                     HIORING* handle);

HRESULT CloseIoRing(HIORING handle);

// Buffer registration
HRESULT BuildIoRingRegisterBuffers(HIORING handle, UINT32 count,
                                   IORING_BUFFER_INFO* buffers, UINT32 index);

// Submit operations
HRESULT BuildIoRingReadFile(HIORING handle, IORING_HANDLE_REF fileRef,
                           IORING_BUFFER_REF bufferRef, UINT32 size,
                           UINT64 fileOffset, UINT_PTR userData, IORING_SQE_FLAGS flags);

HRESULT BuildIoRingWriteFile(HIORING handle, IORING_HANDLE_REF fileRef,
                            IORING_BUFFER_REF bufferRef, UINT32 size,
                            UINT64 fileOffset, UINT_PTR userData, IORING_SQE_FLAGS flags);

HRESULT SubmitIoRing(HIORING handle, UINT32 waitOperations, UINT32 milliseconds,
                     UINT32* submitted);

// Completion processing
HRESULT PopIoRingCompletion(HIORING handle, IORING_CQE* cqe);
```

For networking, we'll use:
- `WSASocket` for creating raw sockets
- `BuildIoRingRegisterFileHandles` to register socket handles
- Custom read/write operations mapped to recv/send semantics

## Implementation Phases

### Phase 1: Foundation (Week 1-2)

**Goal**: Create project structure and P/Invoke declarations

#### Deliverables:

1. **Project Setup**
   ```
   src/Magma.IoRing/
   ├── Magma.IoRing.csproj
   ├── README.md
   ├── Interop/
   │   ├── IoRingApi.cs          # P/Invoke declarations
   │   ├── Winsock2.cs           # Socket API declarations
   │   └── Constants.cs          # API constants and enums
   └── IoRingVersion.cs
   ```

2. **Core P/Invoke Bindings**
   - `HIORING` handle type
   - `IORING_CREATE_FLAGS` enumeration
   - `IORING_CQE` and `IORING_SQE` structures
   - Function declarations with `LibraryImport` for better performance

3. **Basic Tests**
   - IoRing creation/destruction
   - Buffer registration
   - Simple file I/O operations (validation)

**Success Criteria**: Can create and destroy IORING handle, register buffers

---

### Phase 2: Buffer Management (Week 3)

**Goal**: Implement efficient memory management for packet buffers

#### Deliverables:

1. **IoRingMemoryManager**
   ```csharp
   public class IoRingMemoryManager : IDisposable
   {
       // Pre-allocated buffer pool
       private readonly Memory<byte>[] _buffers;
       private readonly GCHandle[] _pinnedHandles;

       public Memory<byte> AllocateBuffer(int size);
       public void RegisterBuffers(HIORING handle);
       public void ReleaseBuffer(Memory<byte> buffer);
   }
   ```

2. **Buffer Pool Configuration**
   - Configurable buffer count (default: 4096)
   - Configurable buffer size (default: 2048 bytes for Ethernet frames)
   - Pinned memory for zero-copy operations
   - Free list management

3. **Tests**
   - Buffer allocation/deallocation
   - Concurrent access patterns
   - Memory leak detection

**Success Criteria**: Can allocate, register, and manage buffer pools efficiently

---

### Phase 3: Socket Integration (Week 4-5)

**Goal**: Integrate IoRing with raw sockets for packet I/O

#### Deliverables:

1. **Raw Socket Setup**
   ```csharp
   public class IoRingSocket : IDisposable
   {
       private readonly IntPtr _socket;
       private readonly HIORING _ioRing;

       public IoRingSocket(AddressFamily family, SocketType type, ProtocolType protocol);
       public void RegisterWithIoRing();
       public bool TryReceive(out Memory<byte> packet);
       public ValueTask SendAsync(ReadOnlyMemory<byte> packet);
   }
   ```

2. **Receive Path**
   - Post multiple receive operations to submission queue
   - Poll completion queue for incoming packets
   - Handle partial receives and errors
   - Replenish receive buffers

3. **Transmit Path**
   - Queue transmit operations
   - Batch submission for efficiency
   - Track completion for buffer reuse
   - Handle backpressure

4. **Tests**
   - Socket creation with IoRing
   - Basic send/receive operations
   - Performance benchmarks

**Success Criteria**: Can send and receive packets through IoRing-registered sockets

---

### Phase 4: Transport Implementation (Week 6-7)

**Goal**: Implement full Magma transport integration

#### Deliverables:

1. **IoRingTransport**
   ```csharp
   public class IoRingTransport : IDisposable
   {
       public IoRingTransport(IPEndPoint endpoint, IoRingTransportOptions options);
       public Task BindAsync(CancellationToken cancellationToken = default);
       public void Dispose();
   }
   ```

2. **IoRingPort**
   ```csharp
   public class IoRingPort<TReceiver> : IDisposable
       where TReceiver : IPacketReceiver
   {
       private readonly IoRingTransmitRing _transmitRing;
       private readonly Thread _receiveThread;

       public IoRingPort(IoRingTransportOptions options, ...);
       public void Start();
       public void Stop();
   }
   ```

3. **IoRingTransmitRing**
   ```csharp
   public class IoRingTransmitRing : IPacketTransmitter
   {
       public bool TryGetNextBuffer(int size, out Memory<byte> memory);
       public ValueTask SendBuffer(int size);
       public uint RandomSequenceNumber();
   }
   ```

4. **Configuration**
   ```csharp
   public class IoRingTransportOptions
   {
       public int SubmissionQueueSize { get; set; } = 2048;
       public int CompletionQueueSize { get; set; } = 2048;
       public int BufferCount { get; set; } = 4096;
       public int BufferSize { get; set; } = 2048;
       public ProtocolType Protocol { get; set; } = ProtocolType.Raw;
       public bool UseRegisteredBuffers { get; set; } = true;
   }
   ```

5. **Integration Tests**
   - Full packet send/receive flow
   - Integration with TcpTransportReceiver
   - Multi-threaded scenarios
   - Error handling and recovery

**Success Criteria**: IoRingTransport can be used as drop-in replacement for AF_XDP/WinTun

---

### Phase 5: Optimization & Hardening (Week 8-9)

**Goal**: Performance optimization and production readiness

#### Deliverables:

1. **Performance Optimizations**
   - Batch SQE submission (submit multiple at once)
   - Prefetch completion queue entries
   - NUMA-aware buffer allocation
   - Lock-free queue for transmit path
   - Adaptive polling vs. blocking wait

2. **Advanced Features**
   - Multi-queue support (separate IoRing per CPU core)
   - Interrupt coalescing configuration
   - Dynamic buffer pool sizing
   - Statistics and monitoring
   - Detailed error diagnostics

3. **Error Handling**
   - Graceful degradation on API failures
   - Socket error recovery
   - Buffer exhaustion handling
   - Timeout management

4. **Benchmarks**
   ```
   benchmarks/Magma.IoRing.Benchmarks/
   ├── IoRingThroughputBenchmark.cs
   ├── IoRingLatencyBenchmark.cs
   └── ComparisonBenchmark.cs (vs WinTun)
   ```

5. **Documentation**
   - Updated README.md with setup instructions
   - Performance tuning guide
   - Troubleshooting section
   - Architecture diagrams

**Success Criteria**:
- 10M+ packets/sec throughput (64-byte packets)
- <10μs median latency
- Comparable to AF_XDP performance on Linux

---

### Phase 6: Testing & Validation (Week 10)

**Goal**: Comprehensive testing and documentation

#### Deliverables:

1. **Test Coverage**
   ```
   test/Magma.IoRing.Facts/
   ├── IoRingApiTests.cs          # P/Invoke validation
   ├── IoRingMemoryTests.cs       # Memory management
   ├── IoRingSocketTests.cs       # Socket operations
   ├── IoRingTransportTests.cs    # End-to-end tests
   └── IoRingPerformanceTests.cs  # Performance validation
   ```

2. **Sample Applications**
   ```
   samples/IoRingSample/
   ├── Program.cs                 # Basic usage example
   └── README.md                  # Sample documentation
   ```

3. **Integration Documentation**
   - Update `docs/ARCHITECTURE.md` with IoRing section
   - Update `docs/INTEGRATION_GUIDE.md` with IoRing examples
   - Create comparison matrix (WinTun vs IoRing)

4. **CI/CD Integration**
   - Windows 11 build agents
   - Automated tests in CI pipeline
   - Performance regression tests

**Success Criteria**: 90%+ code coverage, all tests passing on Windows 11

## Technical Challenges & Solutions

### Challenge 1: Socket Type for Raw Ethernet

**Problem**: IoRing works with file handles/sockets. Raw Ethernet access on Windows is limited.

**Solutions**:
1. **Raw Sockets (AF_INET, SOCK_RAW)**: Provides IP-level access
   - Pros: Standard Windows API, works with IoRing
   - Cons: Limited to IP packets, requires admin privileges
   - Use case: IP/TCP/UDP layer processing

2. **NDIS Protocol Driver**: Full Ethernet frame access
   - Pros: Complete control, true Layer 2 access
   - Cons: Requires kernel driver development, complex
   - Defer to future phase

3. **WinPcap/Npcap with Custom Driver**: Existing solutions
   - Pros: Proven technology
   - Cons: Third-party dependency, may not integrate with IoRing
   - Not recommended for initial implementation

**Recommended Approach**: Start with Raw Sockets (SOCK_RAW) for IP-level access, similar to how AF_XDP can work at IP layer. This provides immediate value for TCP/UDP workloads.

### Challenge 2: Buffer Registration Limits

**Problem**: IoRing has limits on registered buffers and handles.

**Solution**:
- Use dynamic buffer registration/deregistration
- Implement buffer pools with rotation
- Monitor IORING_FEATURE_FLAGS for capabilities
- Fall back to non-registered buffers if needed

### Challenge 3: API Availability

**Problem**: IoRing requires Windows 11+, limiting compatibility.

**Solution**:
- Runtime detection of IoRing availability
- Graceful fallback to WinTun if IoRing unavailable
- Clear error messages guiding users
- Document minimum OS requirements prominently

```csharp
public static bool IsIoRingSupported()
{
    if (!OperatingSystem.IsWindowsVersionAtLeast(10, 0, 22000))
        return false;

    try
    {
        // Attempt to load API
        _ = NativeLibrary.Load("api-ms-win-core-ioring-l1-1-0.dll");
        return true;
    }
    catch
    {
        return false;
    }
}
```

### Challenge 4: Packet Parsing

**Problem**: Raw sockets may not provide Ethernet headers.

**Solution**:
- When using SOCK_RAW at IP level, synthesize Ethernet headers for Magma's pipeline
- Alternatively, investigate promiscuous mode sockets
- Document limitations clearly

### Challenge 5: Performance Tuning

**Problem**: Achieving AF_XDP-level performance requires careful tuning.

**Solution**:
- Profile hot paths with ETW/PerfView
- Use SIMD for checksum calculations (already in Magma)
- Batch operations aggressively
- Consider CPU affinity for receive thread
- Document tuning parameters

## Testing Strategy

### Unit Tests
- IoRing API wrapper correctness
- Buffer management correctness
- Error handling paths

### Integration Tests
- End-to-end packet transmission
- TCP/UDP protocol integration
- Multi-threaded scenarios
- Resource cleanup

### Performance Tests
- Throughput benchmarks (pps)
- Latency measurements (μs)
- CPU utilization
- Memory usage
- Comparison with WinTun baseline

### Platform Tests
- Windows 11 Pro
- Windows 11 Enterprise
- Windows Server 2022
- Various NIC drivers

## Performance Targets

Based on AF_XDP and io_uring benchmarks:

| Metric | Target | Measurement Method |
|--------|--------|-------------------|
| Throughput | 10M pps | 64-byte UDP packets |
| Latency (median) | <10μs | Round-trip ping |
| Latency (p99) | <50μs | Round-trip ping |
| CPU Efficiency | <30% | Single core @ 1M pps |
| Memory Overhead | <100MB | 4K buffer pool |

## Dependencies

### External Libraries
- None (uses Windows built-in APIs)

### NuGet Packages
- System.Runtime.InteropServices (built-in)
- System.Memory (built-in)

### Platform Requirements
- Windows SDK 10.0.22000.0 or later (for IoRing headers)
- Visual Studio 2022 or .NET 8.0 SDK

## Risks & Mitigations

| Risk | Impact | Likelihood | Mitigation |
|------|--------|------------|------------|
| IoRing API instability | High | Low | Pin to specific Windows version, test thoroughly |
| Limited OS support | Medium | High | Provide WinTun fallback, document requirements |
| Driver compatibility | Medium | Medium | Test with common NIC vendors, document issues |
| Performance below expectations | Medium | Medium | Early benchmarking, optimization phase |
| Raw socket limitations | High | Medium | Start with IP-level, plan NDIS driver for future |
| API bugs in Windows | High | Low | Report to Microsoft, implement workarounds |

## Future Enhancements

### Phase 7+ (Future)
1. **NDIS Protocol Driver**: True Layer 2 access for raw Ethernet
2. **XDP Integration**: Windows XDP (eBPF) support when available
3. **DPDK-like Features**: CPU affinity, huge pages, flow steering
4. **Multi-queue Scaling**: Per-core IoRing instances with RSS
5. **Hardware Offload**: TSO/GSO, checksum offload integration
6. **Zero-copy Send**: Direct NIC memory access if supported

## Success Metrics

The implementation will be considered successful when:

1. **Functional**: Can send/receive packets through IoRing transport
2. **Compatible**: Works with existing Magma TCP/IP stack
3. **Performant**: Achieves >5M pps throughput (better than WinTun)
4. **Reliable**: Passes all tests, no memory leaks, stable under load
5. **Documented**: Clear setup guide, examples, troubleshooting
6. **Maintainable**: Clean code, follows Magma conventions

## Timeline Summary

| Phase | Duration | Deliverable |
|-------|----------|-------------|
| Phase 1: Foundation | 2 weeks | P/Invoke bindings, project structure |
| Phase 2: Buffer Management | 1 week | Memory manager, buffer pools |
| Phase 3: Socket Integration | 2 weeks | Raw socket + IoRing integration |
| Phase 4: Transport | 2 weeks | Full IPacketTransmitter implementation |
| Phase 5: Optimization | 2 weeks | Performance tuning, hardening |
| Phase 6: Testing | 1 week | Comprehensive tests, documentation |
| **Total** | **10 weeks** | Production-ready IoRing transport |

## Conclusion

Implementing IoRing support in Magma will provide Windows users with a high-performance networking transport comparable to AF_XDP on Linux, without requiring third-party drivers like WinTun. The phased approach ensures steady progress with validation at each step, while the clear architecture aligns with Magma's existing transport abstractions.

The initial implementation focusing on IP-level raw sockets provides immediate value, with future phases potentially adding true Layer 2 access via NDIS protocol drivers. This pragmatic approach balances ambition with deliverability.

## References

- [IoRing API Documentation](https://learn.microsoft.com/en-us/windows/win32/api/ioringapi/)
- [Windows IoRing Overview](https://windows-internals.com/i-o-rings-when-one-i-o-operation-is-not-enough/)
- [Linux io_uring](https://kernel.dk/io_uring.pdf) (design inspiration)
- [AF_XDP Documentation](https://www.kernel.org/doc/html/latest/networking/af_xdp.html) (comparable Linux technology)
- [Raw Sockets on Windows](https://learn.microsoft.com/en-us/windows/win32/winsock/tcp-ip-raw-sockets-2)
- [Windows NDIS Protocol Drivers](https://learn.microsoft.com/en-us/windows-hardware/drivers/network/ndis-protocol-drivers)

---

**Document Version**: 1.0
**Last Updated**: 2026-02-12
**Author**: GitHub Copilot Agent
**Status**: Planning Phase

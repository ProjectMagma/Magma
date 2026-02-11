# Magma.WinTun

WinTun driver integration for high-performance virtual network adapters on Windows.

## Overview

This module provides integration with the WinTun driver, a modern, lightweight virtual network adapter driver maintained by the WireGuard project. WinTun enables creating TUN (Layer 3) interfaces on Windows for VPN applications, custom network stacks, and tunnel implementations.

## WinTun Driver

WinTun is the modern replacement for TAP-Windows and other legacy virtual adapter solutions. It is:

- **Lightweight**: Driver binary is less than 50KB
- **Modern**: Designed for Windows 7 through Windows 11
- **Fast**: Optimized for high throughput and low latency
- **Maintained**: Actively developed by the WireGuard team
- **Signed**: Microsoft-signed driver (WHQL certified)
- **Open Source**: MIT licensed

## Requirements

- Windows 7 or newer
- WinTun driver installed (download from https://www.wintun.net/)
- Administrator privileges to create virtual adapters

## Installation

### 1. Download WinTun Driver

Get the latest WinTun driver from the official website:
- **Official Site**: https://www.wintun.net/
- **GitHub**: https://git.zx2c4.com/wintun/about/

### 2. Install the Driver

WinTun can be deployed in several ways:

**Option A: System-wide installation**
```powershell
# Extract wintun.dll to C:\Windows\System32
# The driver is automatically loaded when needed
```

**Option B: Application bundle**
```
# Include wintun.dll with your application
# Load from application directory
```

### 3. Create Virtual Adapter

Use the WinTun API to create a virtual network adapter programmatically, or use network configuration tools.

## Architecture

```
User Space Application (Magma.WinTun)
         │
         ↓
   WinTun API (wintun.dll)
         │
         ↓ (IOCTL/DeviceIoControl)
   WinTun Driver (wintun.sys)
         │
         ↓
   Windows Network Stack
         │
         ↓
   Virtual Network Adapter
```

## Current Implementation

The current Magma.WinTun implementation uses TAP-Windows compatible interface:

- **Device Access**: Uses Windows device handles via `CreateFile`
- **I/O Operations**: FileStream-based read/write operations
- **Registry Integration**: Discovers adapters via Windows registry
- **Async I/O**: Overlapped I/O for efficient packet processing

### Key Components

- **WinTunPort**: Main port abstraction managing file handles and packet flow
- **WinTunTransitter**: Implements `IPacketTransmitter` for packet transmission
- **WinTunTransportReceiver**: Implements `IPacketReceiver` for packet reception
- **WinTunMemoryPool**: Memory pooling for efficient buffer management

## Migration to Modern WinTun API

For best performance and modern Windows compatibility, consider upgrading to the native WinTun API:

### Native WinTun API Benefits

1. **Ring Buffer Interface**: Direct access to shared memory rings (similar to AF_XDP)
2. **Zero-copy**: Avoid data copies between user/kernel space
3. **Better Performance**: Optimized for modern Windows versions
4. **Cleaner API**: Purpose-built for TUN interfaces

### WinTun API Functions

```csharp
// Create adapter
WintunCreateAdapter(name, tunnelType, requestedGUID)

// Get read/write handles
WintunStartSession(adapter, capacity)

// Receive packets
WintunReceivePacket(session, out packetSize)
WintunReleaseReceivePacket(session, packet)

// Send packets
WintunAllocateSendPacket(session, packetSize)
WintunSendPacket(session, packet)

// Cleanup
WintunEndSession(session)
WintunDeleteAdapter(adapter)
```

## Usage Example

```csharp
// Current implementation (TAP-compatible)
var port = new WinTunPort<TcpTransportReceiver<WinTunTransitter>>(
    adapterName: "Magma TAP",
    packetReceiverFactory: transmitter => 
        new TcpTransportReceiver<WinTunTransitter>(
            endpoint, transmitter, dispatcher
        )
);
```

## Advantages of WinTun over TAP-Windows

| Feature | WinTun | TAP-Windows (Legacy) |
|---------|--------|---------------------|
| Driver Size | < 50KB | > 100KB |
| Performance | Optimized | Moderate |
| Maintenance | Active | Deprecated |
| Windows 10+ | Native support | Compatibility mode |
| API | Modern C API | IOCTL-based |
| License | MIT | GPL (complex) |

## Network Configuration

After creating a WinTun adapter, configure it using standard Windows networking:

```powershell
# Set IP address
netsh interface ip set address "Magma TAP" static 10.0.0.1 255.255.255.0

# Set DNS (optional)
netsh interface ip set dns "Magma TAP" static 8.8.8.8

# Enable interface
netsh interface set interface "Magma TAP" admin=enabled
```

## Troubleshooting

### Driver Not Found
- Ensure wintun.dll is in System32 or application directory
- Check Windows version compatibility (Windows 7+)

### Permission Denied
- Run application as Administrator
- Check User Account Control (UAC) settings

### Performance Issues
- Disable hardware offload on physical adapter if routing through WinTun
- Use native WinTun API instead of TAP compatibility layer
- Consider increasing buffer sizes

## References

- [WinTun Official Site](https://www.wintun.net/)
- [WinTun Source Code](https://git.zx2c4.com/wintun/)
- [WireGuard for Windows](https://www.wireguard.com/install/)
- [WinTun API Documentation](https://git.zx2c4.com/wintun/about/API.md)

## Future Enhancements

Potential improvements for Magma.WinTun:

1. Native WinTun API integration (ring buffer based)
2. Zero-copy packet processing
3. Multi-queue support for parallel processing
4. Better async/await patterns
5. Improved error handling and diagnostics

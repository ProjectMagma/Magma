# Magma UDP Echo Server Sample

A simple UDP echo server demonstrating end-to-end usage of the Magma network stack with UDP transport.

## Overview

This sample application shows how to:
- Parse incoming UDP packets using the Magma stack (Ethernet → IPv4 → UDP)
- Process UDP datagrams at the transport layer
- Construct and transmit UDP response packets
- Use NetMap for high-performance packet I/O on Linux

The server listens on a specified UDP port and echoes back any received data to the sender.

## What It Demonstrates

- **UDP Header Parsing**: Uses `Udp.TryConsume()` to parse UDP headers from raw packet data
- **Packet Construction**: Builds outgoing UDP packets from scratch, including:
  - Ethernet frame with swapped MAC addresses
  - IPv4 header with swapped IP addresses and checksum calculation
  - UDP header with swapped ports
- **Zero-Copy Processing**: Direct manipulation of packet buffers using `Span<byte>` and unsafe pointers
- **NetMap Integration**: High-performance packet I/O using Linux NetMap interface

## Building

From the repository root:

```bash
dotnet build sample/Magma.NetMap.UdpEchoServer
```

## Running

### Prerequisites

- Linux with NetMap kernel module loaded
- Network interface available for NetMap (typically a dedicated interface)
- Root privileges or appropriate capabilities

### Start the Server

```bash
# Run with default settings (eth0, port 7777)
sudo dotnet run --project sample/Magma.NetMap.UdpEchoServer

# Specify interface and port
sudo dotnet run --project sample/Magma.NetMap.UdpEchoServer -- eth1 8888
```

### Test the Server

From another machine on the same network:

```bash
# Using netcat (nc)
echo "Hello Magma!" | nc -u <server-ip> 7777

# Using Python
python3 -c "import socket; s = socket.socket(socket.AF_INET, socket.SOCK_DGRAM); s.sendto(b'Hello', ('<server-ip>', 7777)); print(s.recvfrom(1024))"

# Using socat
echo "Test message" | socat - UDP4:<server-ip>:7777
```

## Architecture

```
┌─────────────────────────────────────────────────────────┐
│                     Application                         │
│                  (UdpEchoReceiver)                      │
└────────────────────┬────────────────────────────────────┘
                     │
                     │ IPacketReceiver.TryConsume()
                     │
┌────────────────────▼────────────────────────────────────┐
│                  NetMap Transport                        │
│           (NetMapPort<UdpEchoReceiver>)                 │
└────────────────────┬────────────────────────────────────┘
                     │
                     │ Direct packet I/O
                     │
┌────────────────────▼────────────────────────────────────┐
│                  Network Interface                       │
│                    (NetMap ring)                         │
└─────────────────────────────────────────────────────────┘
```

## Code Flow

### Receive Path

1. NetMap delivers raw packet buffer to `UdpEchoReceiver.TryConsume()`
2. Parse Ethernet frame: `Ethernet.TryConsume()`
3. Check for IPv4: `etherIn.Ethertype == EtherType.IPv4`
4. Parse IPv4 header: `IPv4.TryConsume()`
5. Check for UDP: `ipIn.Protocol == ProtocolNumber.Udp`
6. Parse UDP header: `Udp.TryConsume()`
7. Check destination port matches listen port

### Transmit Path

1. Get transmit buffer: `_transmitter.TryGetNextBuffer()`
2. Build Ethernet header (swap source/dest MACs)
3. Build IPv4 header (swap source/dest IPs, calculate checksum)
4. Build UDP header (swap source/dest ports)
5. Copy original data payload
6. Send packet: `_transmitter.SendBuffer()` and `ForceFlush()`

## Technical Details

### UDP Header Structure

The `Udp` struct in `Magma.Transport.Udp` represents the 8-byte UDP header:

```
 0                   1                   2                   3
 0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8 9 0 1
+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
|          Source Port          |       Destination Port        |
+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
|            Length             |           Checksum            |
+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
```

- **Source/Destination Port**: 16-bit port numbers
- **Length**: Total length of UDP header + data (minimum 8 bytes)
- **Checksum**: Optional error checking (set to 0 in this sample)

### Performance Characteristics

- **Zero-copy**: Direct buffer manipulation using `Unsafe` and `Span<byte>`
- **Minimal allocations**: Packet buffers are pooled by NetMap
- **Kernel bypass**: NetMap bypasses the Linux network stack for lower latency

## Limitations

- **NetMap only**: Requires Linux with NetMap kernel module
- **No checksum validation**: UDP checksum is not validated on receive
- **No fragmentation**: Only handles packets that fit in a single Ethernet frame
- **Simplified error handling**: Production code would need more robust error checking

## Related Components

- `src/Magma.Transport.Udp` - UDP protocol implementation
- `src/Magma.Internet.Ip` - IPv4/IPv6 headers
- `src/Magma.Link` - Ethernet frame handling
- `src/Magma.NetMap` - NetMap transport integration

## See Also

- [RFC 768 - User Datagram Protocol](https://www.rfc-editor.org/rfc/rfc768)
- [NetMap - Fast packet I/O framework](http://info.iet.unipi.it/~luigi/netmap/)
- Other samples: `sample/Magma.NetMap.TcpHost`, `samples/Magma.NetMap.Host`

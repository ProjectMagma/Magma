using System;
using System.Buffers;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Server.Kestrel.Transport.Abstractions.Internal;
using static Magma.Network.IPAddress;

namespace Magma.Transport.Tcp
{
    public class TcpTransportConnection
    {
        public V4Address _v4Address;
        public int _port;
        public TransportConnection _transport;
        
        public TcpTransportConnection(V4Address address, int port, TransportConnection transport, TcpHandler handler)
        {
            _v4Address = address;
            _port = port;
            _transport = transport;
        }

        public async Task HandlePacket(ReadOnlyMemory<byte> packet, IMemoryOwner<byte> manager)
        {
            await _transport.Input.WriteAsync(packet);
            manager.Dispose();
        }
    }
}

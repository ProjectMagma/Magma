using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using static Magma.Network.IPAddress;

namespace Magma.Transport.Tcp
{
    public class TcpHandler
    {
        private V4Address _listeningAddress;
        private ushort _listeningPort;
        private Dictionary<(V4Address address, uint port), object> _activeConnections = new Dictionary<(V4Address, uint), object>();

        public TcpHandler(IPEndPoint listeningAddress)
        {
            if(listeningAddress == null || listeningAddress.AddressFamily != System.Net.Sockets.AddressFamily.InterNetwork)
            {
                throw new InvalidOperationException("Attempt to use an invalid IPv4 Address");
            }
            _listeningAddress = new V4Address()
            {
                Address = BitConverter.ToUInt32(listeningAddress.Address.GetAddressBytes(), 0)
            };
            _listeningPort = (ushort)listeningAddress.Port;
        }

        public bool HandleTcpFrame(V4Address sourceAddress, V4Address targetAddress, Network.Header.Tcp header, Span<byte> content)
        {
            if (targetAddress != _listeningAddress) return false;
            if (header.DestinationPort != _listeningPort) return false;

            // We have data for us so we need to find if we have a connection
            if(_activeConnections.TryGetValue((sourceAddress, header.SourcePort), out var connection))
            {
                // Here we get the connection state machine to handle the packet and return if it is good or not
            }
            // Should be a new connection as we don't know this port/address combo
            // if it isn't a syn packet we will just swallow and return as its invalid
            if (!header.SYN) return true;

            // It is a syn packet so we need to start a new connection
            _activeConnections[(sourceAddress, header.SourcePort)] = new object();
            throw new NotImplementedException();
        }
    }
}

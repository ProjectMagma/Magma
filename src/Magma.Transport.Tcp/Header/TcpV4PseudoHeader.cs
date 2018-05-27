using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using Magma.Internet.Ip;
using static Magma.Network.IPAddress;

namespace Magma.Transport.Tcp.Header
{
    [StructLayout(LayoutKind.Sequential, Pack =1)]
    public struct TcpV4PseudoHeader
    {
        public V4Address Destination;
        public V4Address Source;
        public byte Reserved;
        public ProtocolNumber ProtocolNumber;
        private ushort _size;

        public ushort Size { get => (ushort)System.Net.IPAddress.NetworkToHostOrder((short)_size); set => _size = (ushort)System.Net.IPAddress.HostToNetworkOrder((short)value); }
    }
}

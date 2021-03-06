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
        public V4Address Source;
        public V4Address Destination;
        public byte Reserved;
        public ProtocolNumber ProtocolNumber;
    }
}

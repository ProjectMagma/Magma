using System;
using System.Collections.Generic;
using System.Text;

namespace Magma.Network
{
    public enum ProtocolNumbers:ushort
    {
        Arp = 0x0806,
        IPv4 = 0x0800,
        IPv6 = 0x86dd,
    }
}

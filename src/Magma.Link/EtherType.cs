using System;
using System.Collections.Generic;
using System.Text;

namespace Magma.Network
{
    public enum EtherType : ushort
    {
        Arp = 0x0608,
        IPv4 = 0x0008,
        IPv6 = 0xdd86,
    }
}

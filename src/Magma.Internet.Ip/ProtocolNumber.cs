using System;
using System.Collections.Generic;
using System.Text;

namespace Magma.Internet.Ip
{
    public enum ProtocolNumber : byte
    {
        Icmp = 0x01,
        Tcp = 0x06,
        Udp = 0x11,

        IPv6Encap = 0x29,
        IPv6Route = 0x2B,
        IPv6Frag = 0x2C,
        IPv6Icmp = 0x3A,
        IPv6NoNxt = 0x3B,
        IPv6Opts = 0x3C
    }
}

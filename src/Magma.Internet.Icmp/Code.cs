using System;
using System.Collections.Generic;
using System.Text;

namespace Magma.Internet.Icmp
{
    public enum Code : ushort
    {
        EchoReply = ControlMessage.EchoReply << 8 | 0x00,

        EchoRequest = ControlMessage.EchoRequest << 8 | 0x00
    }
}

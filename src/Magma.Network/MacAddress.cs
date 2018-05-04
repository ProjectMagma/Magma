using System;
using System.Collections.Generic;
using System.Text;

namespace Magma.Network
{
    public struct MacAddress
    {
        public ushort AddressPart1;
        public ushort AddressPart2;
        public ushort AddressPart3;

        public static readonly MacAddress Broadcast = new MacAddress() { AddressPart1 = 0xFFFF, AddressPart2 = 0xFFFF, AddressPart3 = 0xFFFF };
    }
}

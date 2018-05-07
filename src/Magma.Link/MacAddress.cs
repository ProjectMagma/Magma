using System;
using System.Collections.Generic;
using System.Text;

namespace Magma.Link
{
    public struct MacAddress
    {
        public static readonly MacAddress Broadcast = new MacAddress() { AddressPart1 = 0xFFFF, AddressPart2 = 0xFFFF, AddressPart3 = 0xFFFF };

        public ushort AddressPart1;
        public ushort AddressPart2;
        public ushort AddressPart3;

        public override string ToString()
        {
            return "0x" + AddressPart1.ToString("x") + AddressPart2.ToString("x") + AddressPart3.ToString("x");
        }
    }
}

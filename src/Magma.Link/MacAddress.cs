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
            return "0x" + 
                (AddressPart1 >> 8).ToString("x2") + (AddressPart1 & 0xFF).ToString("x2") +
                (AddressPart2 >> 8).ToString("x2") + (AddressPart2 & 0xFF).ToString("x2") +
                (AddressPart3 >> 8).ToString("x2") + (AddressPart3 & 0xFF).ToString("x2");
        }
    }
}

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using Magma.Link;

namespace Magma.Network.Header
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Ethernet
    {
        public ulong PreambleSfd;
        public MacAddress Destination;
        public MacAddress Source;
        public EtherType Ethertype;
    }
}

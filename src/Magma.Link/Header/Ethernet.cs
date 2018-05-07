using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Magma.Link;

namespace Magma.Network.Header
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Ethernet
    {
        public MacAddress Destination;
        public MacAddress Source;
        public EtherType Ethertype;

        bool TryConsume(ref Span<byte> span, out Ethernet ethernet)
        {
            ethernet = default; // overlay
            return false; // CRC check, trim span
        }
    }
}

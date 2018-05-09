using System;
using System.Collections.Generic;
using System.Text;

namespace Magma.NetMap.Interop
{
    internal static class Consts
    {
        public const uint NIOCREGIF = 0xC03C6992;
        public const uint NIOCTXSYNC = 27028;
        public const uint NIOCRXSYNC = 27029;
        public const int POLLTIME = 1;
        public const ushort NETMAP_API = 12;
    }
}

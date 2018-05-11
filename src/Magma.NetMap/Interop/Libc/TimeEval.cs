using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Magma.NetMap.Interop
{
    internal static partial class Libc
    {
        [StructLayout(LayoutKind.Sequential)]
        internal struct TimeValue
        {
            public ulong tv_sec;
            public ulong tv_usec;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Magma.NetMap.Interop
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct nm_stat
    {
        uint ps_recv;
        uint ps_drop;
        uint ps_ifdrop;
    }
}

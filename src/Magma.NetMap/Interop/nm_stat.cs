using System;
using System.Collections.Generic;
using System.Text;

namespace Magma.NetMap.Interop
{
    internal struct nm_stat
    {
        uint ps_recv;
        uint ps_drop;
        uint ps_ifdrop;
    }
}

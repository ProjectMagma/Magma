using System;
using System.Collections.Generic;
using System.Text;

namespace Magma.NetMap
{
    internal enum NetMapRequestMode : uint
    {
        NR_REG_ALL_NIC = 1,
        NR_REG_SW = 2,
        NR_REG_NIC_SW = 3,
        NR_REG_ONE_NIC = 4,
    }
}

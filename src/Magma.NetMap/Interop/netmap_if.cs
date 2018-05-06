using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Magma.NetMap.Interop
{
    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct netmap_if
    {
        public fixed byte ni_name[16];
        public uint ni_version;
        public uint ni_flags;
        public uint ni_tx_rings;
        public uint ni_rx_rings;
        public uint ni_bufs_head;
        public uint spare1;
        public uint spare2;
        public uint spare3;
        public uint spare4;
        public uint spare5;
    }
}

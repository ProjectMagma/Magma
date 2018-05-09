using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Magma.NetMap.Interop
{
    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct NetMapInterface
    {
        public fixed byte Name[16];
        public uint Version;
        public uint Flags;
        public uint NumberOfTXRings;
        public uint NumberOfRXRings;
        public uint ni_bufs_head;
        public uint spare1;
        public uint spare2;
        public uint spare3;
        public uint spare4;
        public uint spare5;
    }
}

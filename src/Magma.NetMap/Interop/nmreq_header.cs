using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Magma.NetMap
{
    [StructLayout(LayoutKind.Sequential)]
    internal unsafe struct nmreq_header
    {
        public ushort version;
        public nr_reqtype reqType;
        public uint reserved;
        public fixed byte nrname[64];
        public ulong nr_options;
        public void* body;
    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace Magma.NetMap
{
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

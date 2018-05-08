using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Magma.NetMap.Interop
{
    [StructLayout(LayoutKind.Sequential)]
    struct Netmap_ring
    {
        public long buf_ofs;
        public uint num_slots;   /* number of slots in the ring. */
        public uint nr_buf_size;
        public ushort ringid;
        public netmap_ringdirection dir;     /* 0: tx, 1: rx */

        public int head;      /* (u) first user slot */
        public int cur;       /* (u) wakeup point */
        public int tail;      /* (k) first kernel slot */

        public uint flags;

        public timeval ts;		/* (k) time of last *sync() */
    }
}

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Magma.NetMap
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct nmreq_register
    {
        public ulong nr_offset; /* nifp offset in the shared region */
        public ulong nr_memsize;    /* size of the shared region */
        public uint nr_tx_slots;   /* slots in tx rings */
        public uint nr_rx_slots;   /* slots in rx rings */
        public ushort nr_tx_rings;   /* number of tx rings */
        public ushort nr_rx_rings;   /* number of rx rings */
        public ushort nr_mem_id; /* id of the memory allocator */
        public ushort nr_ringid; /* ring(s) we care about */
        public NetMapRequestMode nr_mode;   /* specify NR_REG_* modes */
        public NetMapRequestFlags nr_flags;  /* additional flags (see below) */
                                   /* monitors use nr_ringid and nr_mode to select the rings to monitor */
        public uint nr_extra_bufs; /* number of requested extra buffers */
    }
}

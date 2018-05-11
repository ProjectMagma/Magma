using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using static Magma.NetMap.Interop.Netmap;

namespace Magma.NetMap.Interop
{
    [StructLayout(LayoutKind.Sequential)]
    internal unsafe struct NetMapRequest
    {
        public fixed byte nr_name[16];
        public uint nr_version;    /* API version */
        public uint nr_offset; /* nifp offset in the shared region */
        public uint nr_memsize;    /* size of the shared region */
        public uint nr_tx_slots;   /* slots in tx rings */
        public uint nr_rx_slots;   /* slots in rx rings */
        public ushort nr_tx_rings;   /* number of tx rings */
        public ushort nr_rx_rings;   /* number of rx rings */
        public ushort nr_ringid; /* ring(s) we care about */
        public nr_cmd nr_cmd;
        public ushort nr_arg1;   /* reserve extra rings in NIOCREGIF */
        public ushort nr_arg2;
        public uint nr_arg3;   /* req. extra buffers in NIOCREGIF */
        public NetMapRequestFlags nr_flags;  
        public uint spare1;
    };
}

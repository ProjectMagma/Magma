using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Magma.NetMap.Interop
{
    internal static partial class Netmap
    {
        internal const ushort NETMAP_API = 12;

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
            public NetmapCommand nr_cmd;
            public ushort nr_arg1;   /* reserve extra rings in NIOCREGIF */
            public ushort nr_arg2;
            public uint nr_arg3;   /* req. extra buffers in NIOCREGIF */
            public NetMapRequestFlags nr_flags;
            public uint spare1;
        };

        internal enum NetmapCommand : ushort
        {
            NETMAP_BDG_ATTACH = 1,  /* attach the NIC */
            NETMAP_BDG_DETACH = 2,  /* detach the NIC */
            NETMAP_BDG_REGOPS = 3,  /* register bridge callbacks */
            NETMAP_BDG_LIST = 4,    /* get bridge's info */
            NETMAP_BDG_VNET_HDR = 5,       /* set the port virtio-net-hdr length */
            NETMAP_BDG_NEWIF = 6,   /* create a virtual port */
            NETMAP_BDG_DELIF = 7,   /* destroy a virtual port */
            NETMAP_PT_HOST_CREATE = 8,  /* create ptnetmap kthreads */
            NETMAP_PT_HOST_DELETE = 9,/* delete ptnetmap kthreads */
            NETMAP_BDG_POLLING_ON = 10,/* delete polling kthread */
            NETMAP_BDG_POLLING_OFF = 11,/* delete polling kthread */
            NETMAP_VNET_HDR_GET = 12, /* get the port virtio-net-hdr length */
        }

        internal enum NetmapRingID : ushort
        {
            NETMAP_HW_RING = 0x4000,    /* single NIC ring pair */
            NETMAP_SW_RING = 0x2000,    /* only host ring pair */
            NETMAP_RING_MASK = 0x0fff,  /* the ring number */
            NETMAP_NO_TX_POLL = 0x1000, /* no automatic txsync on poll */
            NETMAP_DO_RX_POLL = 0x8000, /* DO automatic rxsync on poll */
        }
    }
}

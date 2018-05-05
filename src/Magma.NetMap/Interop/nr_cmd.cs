using System;
using System.Collections.Generic;
using System.Text;

namespace Magma.NetMap.Interop
{
    public enum nr_cmd : ushort
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
}

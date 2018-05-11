using System;
using System.Collections.Generic;
using System.Text;

namespace Magma.NetMap
{
    internal enum nr_reqtype : ushort
    {
        /* Register a netmap port with the device. */
        NETMAP_REQ_REGISTER = 1,
        /* Get information from a netmap port. */
        NETMAP_REQ_PORT_INFO_GET,
        /* Attach a netmap port to a VALE switch. */
        NETMAP_REQ_VALE_ATTACH,
        /* Detach a netmap port from a VALE switch. */
        NETMAP_REQ_VALE_DETACH,
        /* List the ports attached to a VALE switch. */
        NETMAP_REQ_VALE_LIST,
        /* Set the port header length (was virtio-net header length). */
        NETMAP_REQ_PORT_HDR_SET,
        /* Get the port header length (was virtio-net header length). */
        NETMAP_REQ_PORT_HDR_GET,
        /* Create a new persistent VALE port. */
        NETMAP_REQ_VALE_NEWIF,
        /* Delete a persistent VALE port. */
        NETMAP_REQ_VALE_DELIF,
        /* Enable polling kernel thread(s) on an attached VALE port. */
        NETMAP_REQ_VALE_POLLING_ENABLE,
        /* Disable polling kernel thread(s) on an attached VALE port. */
        NETMAP_REQ_VALE_POLLING_DISABLE,
        /* Get info about the pools of a memory allocator. */
        NETMAP_REQ_POOLS_INFO_GET,
    }
}

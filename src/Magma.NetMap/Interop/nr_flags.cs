using System;
using System.Collections.Generic;
using System.Text;

namespace Magma.NetMap
{
    internal enum nr_flags : ulong
    {
        NR_MONITOR_TX = 0x100,
        NR_MONITOR_RX = 0x200,
        NR_ZCOPY_MON = 0x400,
        /* request exclusive access to the selected rings */
        NR_EXCLUSIVE = 0x800,
        /* request ptnetmap host support */
        NR_PTNETMAP_HOST = 0x1000,
        NR_RX_RINGS_ONLY = 0x2000,
        NR_TX_RINGS_ONLY = 0x4000,
        /* Applications set this flag if they are able to deal with virtio-net headers,
         * that is send/receive frames that start with a virtio-net header.
         * If not set, NIOCREGIF will fail with netmap ports that require applications
         * to use those headers. If the flag is set, the application can use the
         * NETMAP_VNET_HDR_GET command to figure out the header length. */
        NR_ACCEPT_VNET_HDR = 0x8000,
        /* The following two have the same meaning of NETMAP_NO_TX_POLL and
         * NETMAP_DO_RX_POLL. */
        NR_DO_RX_POLL = 0x10000,
        NR_NO_TX_POLL = 0x20000,
    }
}

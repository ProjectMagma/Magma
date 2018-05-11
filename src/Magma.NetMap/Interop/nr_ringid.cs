using System;
using System.Collections.Generic;
using System.Text;

namespace Magma.NetMap.Interop
{
    enum nr_ringid : ushort
    {
        NETMAP_HW_RING = 0x4000,    /* single NIC ring pair */
        NETMAP_SW_RING = 0x2000,    /* only host ring pair */
        NETMAP_RING_MASK = 0x0fff,  /* the ring number */
        NETMAP_NO_TX_POLL = 0x1000, /* no automatic txsync on poll */
        NETMAP_DO_RX_POLL = 0x8000,	/* DO automatic rxsync on poll */

    }
}

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace Magma.NetMap.Interop
{
    class NetMapInterop
    {
        const ushort NETMAP_RING_MASK = 0x0fff;	/* the ring number */
        const ushort NETMAP_API = 12;
        const ushort NETMAP_NO_TX_POLL = 0x1000;	/* no automatic txsync on poll */
        const ushort NETMAP_DO_RX_POLL = 0x8000;
        const uint NIOCREGIF = 0xC03C6992;
        const ushort NR_REG_MASK = 0xf; /* to extract NR_REG_* mode from nr_flags */

        private enum OpenFlags : int
        {
            O_RDONLY = 0x0000,		/* open for reading only */
            O_WRONLY = 0x0001,		/* open for writing only */
            O_RDWR = 0x0002,		/* open for reading and writing */
            O_ACCMODE = 0x0003,		/* mask for above modes */
        }

        private unsafe nm_desc nm_open(string ifname, nmreq req, ulong flags, void* arg)
        {
            nr_mode nr_reg;

            var ptrToDescription = Marshal.AllocHGlobal(Unsafe.SizeOf<nm_desc>());
            var d = Unsafe.AsRef<nm_desc>(ptrToDescription.ToPointer());
            d.self = ptrToDescription;

            var fd = Unix.Open("/dev/netmap", (int)OpenFlags.O_RDWR);
            if (fd < 0) throw new InvalidOperationException("Need to handle properly (release memory etc)");


            //if (req)

            //    d->req = *req;

            //if (!(new_flags & NM_OPEN_IFNAME))
            //{
            //    if (nm_parse(ifname, d, errmsg) < 0)
            //        goto fail;
            //}
            
            d.req.nr_version = NETMAP_API;
            d.req.nr_ringid &= NETMAP_RING_MASK;

            /* add the *XPOLL flags */
            d.req.nr_ringid |= (ushort)(flags & (NETMAP_NO_TX_POLL | NETMAP_DO_RX_POLL));

            if (Unix.IOCtl(d.fd, NIOCREGIF, &d.req) != 0)
            {
                throw new InvalidOperationException("Some failure to get the port, need better error handling");
            }

            nr_reg = (nr_mode)(d.req.nr_flags & NR_REG_MASK);

            if (nr_reg == nr_mode.NR_REG_SW)
            { /* host stack */
                d.first_tx_ring = d.last_tx_ring = d.req.nr_tx_rings;
                d.first_rx_ring = d.last_rx_ring = d.req.nr_rx_rings;
            }
            else if (nr_reg == nr_mode.NR_REG_ALL_NIC)
            { /* only nic */
                d.first_tx_ring = 0;
                d.first_rx_ring = 0;
                d.last_tx_ring = (ushort)(d.req.nr_tx_rings - 1);
                d.last_rx_ring = (ushort)(d.req.nr_rx_rings - 1);
            }
            else if (nr_reg == nr_mode.NR_REG_NIC_SW)
            {
                d.first_tx_ring = 0;
                d.first_rx_ring = 0;
                d.last_tx_ring = d.req.nr_tx_rings;
                d.last_rx_ring = d.req.nr_rx_rings;
            }
            else if (nr_reg == nr_mode.NR_REG_ONE_NIC)
            {
                /* XXX check validity */
                d.first_tx_ring = d.last_tx_ring =
                d.first_rx_ring = d.last_rx_ring = (ushort)(d.req.nr_ringid & NETMAP_RING_MASK);
            }
            else
            { /* pipes */
                d.first_tx_ring = d.last_tx_ring = 0;
                d.first_rx_ring = d.last_rx_ring = 0;
            }

            d.cur_tx_ring = d.first_tx_ring;
            d.cur_rx_ring = d.first_rx_ring;
            return d;
        }
    }
}

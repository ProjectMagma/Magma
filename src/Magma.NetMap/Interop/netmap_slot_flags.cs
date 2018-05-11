using System;
using System.Collections.Generic;
using System.Text;

namespace Magma.NetMap.Interop
{
    public enum netmap_slot_flags : ushort
    {
        NS_BUF_CHANGED = 0x0001,
        /*
         * must be set whenever buf_idx is changed (as it might be
         * necessary to recompute the physical address and mapping)
         * It is also set by the kernel whenever the buf_idx is
         * changed internally (e.g., by pipes). Applications may
         * use this information to know when they can reuse the
         * contents of previously prepared buffers.
         */
        NS_REPORT = 0x0002, /* ask the hardware to report results */
                            /*
                             * Request notification when slot is used by the hardware.
                             * Normally transmit completions are handled lazily and
                             * may be unreported. This flag lets us know when a slot
                             * has been sent (e.g. to terminate the sender).
                             */
        NS_FORWARD = 0x0004,    /* pass packet 'forward' */
                                /*
                                 * (Only for physical ports, rx rings with NR_FORWARD set).
                                 * Slot released to the kernel (i.e. before ring->head) with
                                 * this flag set are passed to the peer ring (host/NIC),
                                 * thus restoring the host-NIC connection for these slots.
                                 * This supports efficient traffic monitoring or firewalling.
                                 */
        NS_NO_LEARN = 0x0008,   /* disable bridge learning */
                                /*
                                 * On a VALE switch, do not 'learn' the source port for
                                 * this buffer.
                                 */
        NS_INDIRECT = 0x0010,   /* userspace buffer */
                                /*
                                 * (VALE tx rings only) data is in a userspace buffer,
                                 * whose address is in the 'ptr' field in the slot.
                                 */
        NS_MOREFRAG = 0x0020,   /* packet has more fragments */
                                /*
                                 * (VALE ports, ptnetmap ports and some NIC ports, e.g.
                                     * ixgbe and i40e on Linux)
                                 * Set on all but the last slot of a multi-segment packet.
                                 * The 'len' field refers to the individual fragment.
                                 */
        NS_PORT_SHIFT = 8,
        NS_PORT_MASK = (0xff << NS_PORT_SHIFT),
        /*
         * The high 8 bits of the flag, if not zero, indicate the
         * destination port for the VALE switch, overriding
         * the lookup table.
         */
    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace Magma.NetMap.Interop
{
    public unsafe struct nm_pkthdr
    {
        /* first part is the same as pcap_pkthdr */
        timeval  ts;
	    uint caplen;
        uint len;

        ulong flags; /* NM_MORE_PKTS etc */
                     //#define NM_MORE_PKTS	1
        void* d;
	    void* slot;
	    void* buf;
    }
}

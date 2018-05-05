using System;
using System.Collections.Generic;
using System.Text;

namespace Magma.NetMap.Interop
{
    public unsafe struct nm_desc
    {
        const int NM_ERRBUF_SIZE = 512;

        public IntPtr self; /* point to self if netmap. */
        public int fd;
        public void* mem;
        public uint memsize;
        public int done_mmap;  /* set if mem is the result of mmap */
        public IntPtr nifp;
        public ushort first_tx_ring;
        public ushort last_tx_ring;
        public ushort cur_tx_ring;
        public ushort first_rx_ring;
        public ushort last_rx_ring;
        public ushort cur_rx_ring;
        public nmreq req;   /* also contains the nr_name = ifname */
                            //struct nm_pkthdr hdr;

        /*
         * The memory contains netmap_if, rings and then buffers.
         * Given a pointer (e.g. to nm_inject) we can compare with
         * mem/buf_start/buf_end to tell if it is a buffer or
         * some other descriptor in our region.
         * We also store a pointer to some ring as it helps in the
         * translation from buffer indexes to addresses.
         */
        public void* some_ring;
        public void* buf_start;
        public void* buf_end;
        /* parameters from pcap_open_live */
        int snaplen;
        int promisc;
        int to_ms;
        char* errbuf;

        /* save flags so we can restore them on close */
        int if_flags;
        int if_reqcap;
        int if_curcap;

        nm_stat st;
	    fixed byte msg[NM_ERRBUF_SIZE];
    };
}

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using static Magma.Interop.Linux.Libc;
using static Magma.NetMap.Interop.Libc;

namespace Magma.NetMap.Interop
{
    internal static partial class Netmap
    {
        internal const ushort NETMAP_API = 12;

        public unsafe static FileDescriptor OpenNetMap(string interfaceName, int ringId, NetMapRequestFlags flags, out NetMapRequest returnedRequest)
        {
            var fd = Open("/dev/netmap", OpenFlags.O_RDWR);
            if (!fd.IsValid) ExceptionHelper.ThrowInvalidOperation($"Unable to open the /dev/netmap device {fd}");
            var request = new NetMapRequest
            {
                nr_cmd = 0,
                nr_ringid = (ushort)ringId,
                nr_version = NETMAP_API,
                nr_flags = flags,
            };
            var textbytes = Encoding.ASCII.GetBytes(interfaceName + "\0");
            fixed (void* txtPtr = textbytes)
            {
                Buffer.MemoryCopy(txtPtr, request.nr_name, textbytes.Length, textbytes.Length);
            }
            if (IOCtl(fd, IOControlCommand.NIOCREGIF, &request) != 0) ExceptionHelper.ThrowInvalidOperation("Failed to open an FD for a single ring");
            returnedRequest = request;
            return fd;
        }

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
                
        [Flags]
        internal enum NetMapRequestFlags : uint
        {
            NR_REG_ALL_NIC = 1,
            NR_REG_SW = 2,
            NR_REG_NIC_SW = 3,
            NR_REG_ONE_NIC = 4,
            NR_MONITOR_TX = 0x100,
            NR_MONITOR_RX = 0x200,
            NR_ZCOPY_MON = 0x400,
            NR_EXCLUSIVE = 0x800,
            NR_PTNETMAP_HOST = 0x1000,
            NR_RX_RINGS_ONLY = 0x2000,
            NR_TX_RINGS_ONLY = 0x4000,
            NR_ACCEPT_VNET_HDR = 0x8000,
            NR_DO_RX_POLL = 0x10000,
            NR_NO_TX_POLL = 0x20000,
        }
    }
}

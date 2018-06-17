using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using Magma.Interop.Linux;
using static Magma.Infiniband.Interop.IbvDevice;

namespace Magma.Infiniband.Interop
{
    internal static class IbvContext
    {
        [DllImport("libibverbs")]
        public unsafe static extern ibv_context* ibv_open_device(ref ibv_device device);

        [StructLayout(LayoutKind.Sequential)]
        public unsafe struct ibv_context
        {
            public ibv_device* Device;
            public ibv_context_ops Ops;
            public Libc.FileDescriptor Cmd;
            public Libc.FileDescriptor Async;
            public int NumCompVectors;
            public pthreadMutex Mutex;
            public IntPtr AbiCompat;
        }

        [StructLayout(LayoutKind.Sequential, Size = 40)]
        public struct pthreadMutex
        {

        }

        [StructLayout(LayoutKind.Sequential)]
        public unsafe struct ibv_context_ops
        {
            public IntPtr query_device;
            public IntPtr query_port;
            public IntPtr alloc_pd;
            public IntPtr dealloc_pd;
            public IntPtr reg_mr;
            public IntPtr rereg_mr;
            public IntPtr dereg_mr;
            public IntPtr alloc_mw;
            public IntPtr bind_mw;
            public IntPtr dealloc_mw;
            public IntPtr create_cq;
            public IntPtr poll_cq;
            public IntPtr req_notify_cq;
            public IntPtr cq_event;
            public IntPtr resize_cq;
            public IntPtr destroy_cq;
            public IntPtr create_sqr;
            public IntPtr modify_sqr;
            public IntPtr query_sqr;
            public IntPtr destroy_sqr;
            public IntPtr post_srq_recv;
            public IntPtr create_qp;
            public IntPtr query_qp;
            public IntPtr modify_qp;
            public IntPtr destroy_qp;
            public IntPtr post_send;
            public IntPtr post_receive;
            public IntPtr create_ah;
            public IntPtr destroy_ah;
            public IntPtr attach_mcast;
            public IntPtr detach_mcast;
            public IntPtr async_event;
        }
    }
}

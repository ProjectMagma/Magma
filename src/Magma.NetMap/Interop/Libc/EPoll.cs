using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Magma.NetMap.Interop
{
    internal static partial class Libc
    {
        [DllImport("libc", EntryPoint = "epoll_wait")]
        internal extern static int EPollWait(EPollHandle __epfd, ref EPollEvent __events, int __maxevents, int __timeout);

        [DllImport("libc", EntryPoint = "epoll_ctl")]
        internal extern static int EPollControl(EPollHandle __epfd, EPollCommand __op, FileDescriptor __fd, ref EPollEvent __event);

        [DllImport("libc", EntryPoint = "epoll_create1")]
        internal extern static EPollHandle EPollCreate(int __flags);

        internal struct EPollHandle
        {
            public int Pointer;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct EPollData
        {
            public IntPtr ptr;
            public FileDescriptor FileDescriptor;
            public uint u32;
            public ulong u64;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct EPollEvent
        {
            public EPollEvents events;        /* Epoll events */
            public EPollData data;        /* User data variable */
        }

        internal enum EPollCommand : uint
        {
            EPOLL_CTL_ADD = 1,
            EPOLL_CTL_DEL = 2,
            EPOLL_CTL_MOD = 3,
        }

        [Flags]
        internal enum EPollEvents : uint
        {
            EPOLLIN = 0x001,
            EPOLLPRI = 0x002,
            EPOLLOUT = 0x004,
            EPOLLRDNORM = 0x040,
            EPOLLRDBAND = 0x080,
            EPOLLWRNORM = 0x100,
            EPOLLWRBAND = 0x200,
            EPOLLMSG = 0x400,
            EPOLLERR = 0x008,
            EPOLLHUP = 0x010,
            EPOLLRDHUP = 0x2000,
            EPOLLEXCLUSIVE = 1u << 28,
            EPOLLWAKEUP = 1u << 29,
            EPOLLONESHOT = 1u << 30,
            EPOLLET = 1u << 31
        }
    }
}

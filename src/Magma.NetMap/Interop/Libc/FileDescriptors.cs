using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using static Magma.NetMap.Interop.Netmap;

namespace Magma.NetMap.Interop
{
    internal static partial class Libc
    {
        [DllImport("libc", EntryPoint = "open")]
        internal static extern FileDescriptor Open([MarshalAs(UnmanagedType.LPStr)] string fileName, OpenFlags flags);

        [DllImport("libc", EntryPoint = "close")]
        internal static extern int Close(FileDescriptor fd);

        [DllImport("libc", EntryPoint = "ioctl")]
        internal static extern int IOCtl(FileDescriptor descriptor, IOControlCommand request, ref NetMapRequest data);

        [DllImport("libc", EntryPoint = "ioctl")]
        internal static extern int IOCtl(FileDescriptor descriptor, IOControlCommand request, IntPtr ptr);


        internal struct FileDescriptor
        {
            public int Descriptor;
            public bool IsValid => Descriptor > 0;
        }

        internal enum IOControlCommand : uint
        {
            NIOCREGIF = 0xC03C6992,
            NIOCTXSYNC = 27028,
            NIOCRXSYNC = 27029,
        }

        [Flags]
        internal enum OpenFlags
        {
            O_RDONLY = 0x0000,      /* open for reading only */
            O_WRONLY = 0x0001,      /* open for writing only */
            O_RDWR = 0x0002,        /* open for reading and writing */
            O_ACCMODE = 0x0003,     /* mask for above modes */
        }
    }
}

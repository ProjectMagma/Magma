using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Magma.Interop.Linux
{
    public static partial class Libc
    {
        [DllImport("libc", EntryPoint = "read")]
        internal unsafe static extern int Read(FileDescriptor fileDescriptor, void* buffer, long size);

        [DllImport("libc", EntryPoint = "write")]
        public unsafe static extern int Write(FileDescriptor fileDescriptor, void* buffer, long size);

        [DllImport("libc", EntryPoint = "open")]
        public static extern FileDescriptor Open([MarshalAs(UnmanagedType.LPStr)] string fileName, OpenFlags flags);

        [DllImport("libc", EntryPoint = "close")]
        public static extern int Close(FileDescriptor fd);
                
        [DllImport("libc", EntryPoint = "ioctl")]
        public unsafe static extern int IOCtl(FileDescriptor descriptor, IOControlCommand request, void* ptr);

        [StructLayout(LayoutKind.Sequential)]
        public struct FileDescriptor
        {
            public int Descriptor;
            public bool IsValid => Descriptor > 0;
        }

        public enum IOControlCommand : uint
        {
            NIOCREGIF = 0xC03C6992,
            NIOCTXSYNC = 27028,
            NIOCRXSYNC = 27029,
        }

        [Flags]
        public enum OpenFlags
        {
            O_RDONLY = 0x0000,      /* open for reading only */
            O_WRONLY = 0x0001,      /* open for writing only */
            O_RDWR = 0x0002,        /* open for reading and writing */
            O_ACCMODE = 0x0003,     /* mask for above modes */
        }
    }
}

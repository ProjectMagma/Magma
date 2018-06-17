using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Magma.Interop.Linux
{
    public static partial class Libc
    {
        [DllImport("libc", EntryPoint = "mmap")]
        public static extern IntPtr MMap(IntPtr addr, ulong length, MemoryMappedProtections prot, MemoryMappedFlags flags, FileDescriptor fd, ulong offset);

        [DllImport("libc", EntryPoint = "munmap")]
        public static extern int MUnmap(IntPtr addr, ulong length);

        [Flags]
        public enum MemoryMappedProtections
        {
            PROT_NONE = 0x0,
            PROT_READ = 0x1,
            PROT_WRITE = 0x2,
            PROT_EXEC = 0x4
        }

        [Flags]
        public enum MemoryMappedFlags
        {
            MAP_SHARED = 0x01,
            MAP_PRIVATE = 0x02,
            MAP_ANONYMOUS = 0x20,
        }
    }
}

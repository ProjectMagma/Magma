using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace Magma.NetMap.Interop
{
    internal static partial class Libc
    {
        [DllImport("libc", EntryPoint = "eventfd_read")]
        internal static extern int EventFDRead(FileDescriptor descriptor, ref ulong value);

        [DllImport("libc", EntryPoint = "eventfd_write")]
        internal static extern int EventFDWrite(FileDescriptor descriptor, ulong value);

        [DllImport("libc", EntryPoint = "eventfd")]
        internal static extern FileDescriptor CreateEventFD(int count, EventFDFlags flags);

        [Flags]
        internal enum EventFDFlags : int
        {
            EFD_SEMAPHORE = 0x00000001,
            EFD_CLOEXEC = 0x02000000,
            EFD_NONBLOCK = 0x00004000
        }
    }
}

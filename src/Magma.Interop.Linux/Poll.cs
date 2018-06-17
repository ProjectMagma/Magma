using System;
using System.Runtime.InteropServices;

namespace Magma.Interop.Linux
{
    public static partial class Libc
    {
        [DllImport("libc", EntryPoint = "poll")]
        public static extern int Poll(ref PollFileDescriptor pollfd, int numberOfFileDescriptors, int timeout);

        [DllImport("libc", EntryPoint = "poll")]
        public unsafe static extern int Poll(PollFileDescriptor* pollfd, int numberOfFileDescriptors, int timeout);

        [StructLayout(LayoutKind.Sequential)]
        public struct PollFileDescriptor
        {
            public FileDescriptor Fd;
            public PollEvents Events;
            public PollEvents Revents;
        }

        [Flags]
        public enum PollEvents : short
        {
            POLLIN = 0x0001, // There is data to read
            POLLPRI = 0x0002, // There is urgent data to read
            POLLOUT = 0x0004, // Writing now will not block
            POLLERR = 0x0008, // Error condition
            POLLHUP = 0x0010, // Hung up
            POLLNVAL = 0x0020, // Invalid request; fd not open
                               // XPG4.2 definitions (via _XOPEN_SOURCE)
            POLLRDNORM = 0x0040, // Normal data may be read
            POLLRDBAND = 0x0080, // Priority data may be read
            POLLWRNORM = 0x0100, // Writing now will not block
            POLLWRBAND = 0x0200, // Priority data may be written
        }
    }
}

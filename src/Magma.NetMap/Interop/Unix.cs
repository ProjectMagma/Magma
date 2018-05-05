using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Magma.NetMap
{
    internal class Unix
    {
        [DllImport("libc", EntryPoint = "open")]
        public static extern int Open([MarshalAs(UnmanagedType.LPStr)] string fileName, int flags);

        [DllImport("libc", EntryPoint = "ioctl", SetLastError = true)]
        public unsafe static extern int IOCtl(int descriptor, uint request, void* data);
    }
}

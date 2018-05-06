using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Magma.NetMap.Interop
{
    [StructLayout(LayoutKind.Sequential)]
    public struct netmap_slot
    {
        public uint buf_idx;   /* buffer index */
        public ushort len;       /* length for this slot */
        public ushort flags;     /* buf changed, etc. */
        public IntPtr ptr;		/* pointer for indirect buffers */
    }
}

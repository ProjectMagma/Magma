using System;
using System.Runtime.InteropServices;
using static Magma.NetMap.Interop.Libc;

namespace Magma.NetMap.Interop
{
    internal static partial class Netmap
    {
        [StructLayout(LayoutKind.Sequential)]
        internal struct NetmapRing
        {
            public long BuffersOffset;
            public uint NumberOfSlotsPerRing;
            public uint BufferSize;
            public ushort RingId;
            public NetmapRingDirection dir;
            public int Head;      
            public int Cursor;     
            public int Tail;      
            public uint Flags;
            public TimeValue LastSyncTimestamp;      
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct NetmapSlot
        {
            public uint buf_idx;   /* buffer index */
            public ushort len;       /* length for this slot */
            public NetmapSlotFlags flags;     /* buf changed, etc. */
            public IntPtr ptr;      /* pointer for indirect buffers */
        }

        [Flags]
        internal enum NetmapSlotFlags : ushort
        {
            NS_BUF_CHANGED = 0x0001,
            NS_REPORT = 0x0002,
            NS_FORWARD = 0x0004,    
            NS_NO_LEARN = 0x0008,   
            NS_INDIRECT = 0x0010,   
            NS_MOREFRAG = 0x0020,  
            NS_PORT_SHIFT = 8,
            NS_PORT_MASK = (0xff << NS_PORT_SHIFT),
        }
                
        internal enum NetmapRingDirection : ushort
        {
            tx = 1,
            rx = 0,
        }
    }
}

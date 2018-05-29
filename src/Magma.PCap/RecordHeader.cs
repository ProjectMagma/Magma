using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Magma.PCap
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct RecordHeader
    {
        public uint ts_sec;         /* timestamp seconds */
        public uint ts_usec;        /* timestamp microseconds */
        public int incl_len;       /* number of octets of packet saved in file */
        public int orig_len;       /* actual length of packet */
    }
}

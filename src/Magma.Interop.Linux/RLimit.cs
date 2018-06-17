using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Magma.Interop.Linux
{
    public static partial class Libc
    {
        [DllImport("libc", EntryPoint = "getrlimit")]
        public static extern int GetRLimit(RLimitResource resource, out RLimit result);

        public enum RLimitResource
        {
            RLIMIT_AS = 0x9,
            RLIMIT_CORE = 0x4,
            RLIMIT_CPU = 0x0,
            RLIMIT_DATA = 0x2,
            RLIMIT_FSIZE = 0x1,
            RLIMIT_MEMLOCK = 0x8,
            RLIMIT_MSGQUEUE = 0xc,
            RLIMIT_NICE = 0xd,
            RLIMIT_NOFILE = 0x7,
            RLIMIT_NPROC = 0x6,
            RLIMIT_RSS = 0x5,
            RLIMIT_RTPRIO = 0xe,
            RLIMIT_SIGPENDING = 0xb,
            RLIMIT_STACK = 0x3,
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct RLimit
        {
            public long Current;
            public long Max;

            public const long Infinity = -1;
        }
    }
}

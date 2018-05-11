using System;
using System.Runtime.CompilerServices;

namespace Magma.Network
{
    public static class Checksum
    {
        public static bool IsValid(ref byte buffer, int length)
            => Calcuate(ref buffer, length) == 0 ? true : false;

        public static ushort Calcuate(ref byte buffer, int length)
        {
            var remaining = length;
            ref var ptr = ref buffer;

            ulong sum = 0;

            while (remaining >= sizeof(ulong))
            {
                remaining -= 8;

                var ulong0 = Unsafe.As<byte, ulong>(ref ptr);
                ptr = ref Unsafe.Add(ref ptr, 8);

                sum += ulong0;
                if (sum < ulong0)
                {
                    sum++;
                }
            }

            if ((remaining & 4) != 0)
            {
                var uint0 = Unsafe.As<byte, uint>(ref ptr);
                ptr = ref Unsafe.Add(ref ptr, 4);

                sum += uint0;
                if (sum < uint0)
                {
                    sum++;
                }
            }

            if ((remaining & 2) != 0)
            {
                var ushort0 = Unsafe.As<byte, ushort>(ref ptr);
                ptr = ref Unsafe.Add(ref ptr, 2);

                sum += ushort0;
                if (sum < ushort0)
                {
                    sum++;
                }
            }

            if ((remaining & 1) != 0)
            {
                var byte0 = ptr;

                sum += byte0;
                if (sum < byte0)
                {
                    sum++;
                }
            }

            // Fold down to 16 bits
            var uint1 = (uint)sum;
            var uint2 = (uint)(sum >> 32);
            uint1 += uint2;
            if (uint1 < uint2)
            {
                uint1++;
            }

            var ushort1 = (ushort)uint1;
            var ushort2 = (ushort)(uint1 >> 16);
            ushort1 += ushort2;
            if (ushort1 < ushort2)
            {
                ushort1++;
            }

            // Invert to get ones-complement result 
            return (ushort)~ushort1;
        }
    }
}

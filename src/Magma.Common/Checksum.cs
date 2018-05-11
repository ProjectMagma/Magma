using System;
using System.Runtime.CompilerServices;

namespace Magma.Network
{
    public static class Checksum
    {
        public static bool IsValid(ref byte buffer, int length)
            => Calcuate(ref buffer, length) == 0 ? true : false;

        // Internet Checksum as defined by RFC 791, RFC 793, RFC 1071, RFC 1141, RFC 1624
        public static ushort Calcuate(ref byte buffer, int length)
        {
            ref var current = ref buffer;
            ulong sum = 0;

            while (length >= sizeof(ulong))
            {
                length -= sizeof(ulong);

                var ulong0 = Unsafe.As<byte, ulong>(ref current);
                current = ref Unsafe.Add(ref current, sizeof(ulong));

                // Add with carry
                sum += ulong0;
                if (sum < ulong0)
                {
                    sum++;
                }
            }

            if ((length & sizeof(uint)) != 0)
            {
                var uint0 = Unsafe.As<byte, uint>(ref current);
                current = ref Unsafe.Add(ref current, sizeof(uint));

                // Add with carry
                sum += uint0;
                if (sum < uint0)
                {
                    sum++;
                }
            }

            if ((length & sizeof(ushort)) != 0)
            {
                var ushort0 = Unsafe.As<byte, ushort>(ref current);
                current = ref Unsafe.Add(ref current, sizeof(ushort));

                // Add with carry
                sum += ushort0;
                if (sum < ushort0)
                {
                    sum++;
                }
            }

            if ((length & sizeof(byte)) != 0)
            {
                var byte0 = current;

                // Add with carry
                sum += byte0;
                if (sum < byte0)
                {
                    sum++;
                }
            }

            // Fold down to 16 bits

            var uint1 = (uint)(sum >> 32);
            var uint2 = (uint)sum;

            // Add with carry
            uint1 += uint2;
            if (uint1 < uint2)
            {
                uint1++;
            }

            var ushort2 = (ushort)uint1;
            var ushort1 = (ushort)(uint1 >> 16);

            // Add with carry
            ushort1 = (ushort)(ushort1 + ushort2);
            if (ushort1 < ushort2)
            {
                ushort1++;
            }

            // Invert to get ones-complement result 
            return (ushort)~ushort1;
        }
    }
}

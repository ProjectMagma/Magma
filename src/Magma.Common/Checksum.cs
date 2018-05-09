using System;

namespace Magma.Network
{
    public static class Checksum
    {
        public unsafe static ushort Calcuate<T>(in T buffer, int length)
            where T : unmanaged
        {
            fixed (T* ptr = &buffer)
            {
                var pByte = (byte*)ptr;

                long sum = 0;

                var pLong = (long*)ptr;
                while (length >= sizeof(long))
                {
                    var s = *pLong++;
                    sum += s;
                    if (sum < s) sum++;
                    length -= 8;
                }

                pByte = (byte*)pLong;
                if ((length & 4) != 0)
                {
                    var s = *(uint*)pByte;
                    sum += s;
                    if (sum < s) sum++;
                    pByte += 4;
                }

                if ((length & 2) != 0)
                {
                    var s = *(ushort*)pByte;
                    sum += s;
                    if (sum < s) sum++;
                    pByte += 2;
                }

                if ((length & 1) != 0)
                {
                    var s = *pByte;
                    sum += s;
                    if (sum < s) sum++;
                }

                /* Fold down to 16 bits */
                var t1 = (uint)sum;
                var t2 = (uint)(sum >> 32);
                t1 += t2;
                if (t1 < t2) t1++;
                var t3 = (ushort)t1;
                var t4 = (ushort)(t1 >> 16);
                t3 += t4;
                if (t3 < t4) t3++;

                return (ushort)~t3;
            }
        }
    }
}

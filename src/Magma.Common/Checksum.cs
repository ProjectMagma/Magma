using System;

namespace Magma.Network
{
    public static class SpanExtensions
    {
        public unsafe static ushort Checksum<T>(in T buffer, int length)
            where T : unmanaged
        {
            var i = 0;
            uint sum = 0;
            uint data = 0;
            fixed (T* ptr = &buffer)
            {
                var pBuffer = (byte*)ptr;
                while (length > 1)
                {
                    data = 0;
                    data = (uint)(
                    ((uint)(pBuffer[i]) << 8)
                    |
                    ((uint)(pBuffer[i + 1]) & 0xFF)
                    );

                    sum += data;
                    if ((sum & 0xFFFF0000) > 0)
                    {
                        sum = sum & 0xFFFF;
                        sum += 1;
                    }

                    i += 2;
                    length -= 2;
                }

                if (length > 0)
                {
                    sum += (uint)(pBuffer[i] << 8);
                    //sum += (UInt32)(buffer[i]);
                    if ((sum & 0xFFFF0000) > 0)
                    {
                        sum = sum & 0xFFFF;
                        sum += 1;
                    }
                }
            }
            sum = ~sum;
            sum = sum & 0xFFFF;
            return (ushort)sum;

        }


        /* Compute the checksum of the given ip header. */
        //        unsafe static int checksum(byte* data, ushort len, int sum)
        //{
        //const uint8_t* addr = data;
        //        uint32_t i;

        ///* Checksum all the pairs of bytes first... */
        //for (i = 0; i<(len & ~1U); i += 2) {
        //sum += (u_int16_t) ntohs(*((u_int16_t*)(addr + i)));
        //if (sum > 0xFFFF)
        //sum -= 0xFFFF;
        //}
        ///*
        // * If there's a single byte left over, checksum it, too.
        // * Network byte order is big-endian, so the remaining byte is
        // * the high byte.
        // */
        //if (i<len) {
        //sum += addr[i] << 8;
        //if (sum > 0xFFFF)
        //sum -= 0xFFFF;
        //}
        //return sum;
        //}
        public static ushort Checksum(this ReadOnlySpan<byte> buffer)
        {
            var length = buffer.Length;
            var i = 0;
            uint sum = 0;
            uint data = 0;
            while (length > 1)
            {
                data = 0;
                data = (uint)(
                ((uint)(buffer[i]) << 8)
                |
                ((uint)(buffer[i + 1]) & 0xFF)
                );

                sum += data;
                if ((sum & 0xFFFF0000) > 0)
                {
                    sum = sum & 0xFFFF;
                    sum += 1;
                }

                i += 2;
                length -= 2;
            }

            if (length > 0)
            {
                sum += (uint)(buffer[i] << 8);
                //sum += (UInt32)(buffer[i]);
                if ((sum & 0xFFFF0000) > 0)
                {
                    sum = sum & 0xFFFF;
                    sum += 1;
                }
            }
            sum = ~sum;
            sum = sum & 0xFFFF;
            return (ushort)sum;

        }
    }
}

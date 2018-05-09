using System;

namespace Magma.Network
{
    public static class SpanExtensions
    {
        public unsafe static ushort Checksum<T>(in T buffer, int length)
            where T : unmanaged
        {
            var sum = 0;
            fixed (T* ptr = &buffer)
            {
                var pBuffer = (byte*)ptr;
                var len = (int)(((uint)length) & ~1U);
                var i = 0;
                for (; i < len; i += 2)
                {
                    sum += (ushort)System.Net.IPAddress.NetworkToHostOrder(*((short*)(pBuffer + i)));
                    if (sum > 0xFFFF)
                        sum -= 0xFFFF;
                }
                /*
                 * If there's a single byte left over, checksum it, too.
                 * Network byte order is big-endian, so the remaining byte is
                 * the high byte.
                 */
                if (i < length)
                {
                    sum += pBuffer[i] << 8;
                    if (sum > 0xFFFF)
                        sum -= 0xFFFF;
                }
                return (ushort)System.Net.IPAddress.HostToNetworkOrder((short)sum);
            }
        }
    }
}

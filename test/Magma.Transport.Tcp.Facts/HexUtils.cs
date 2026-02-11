using System;
using System.Linq;

namespace Magma.Transport.Tcp.Facts
{
    public static class HexUtils
    {
        public static byte[] HexToByteArray(this string hex)
        {
            hex = string.Join("", hex.Where(c => !char.IsWhiteSpace(c) && c != '-'));
            var numberChars = hex.Length;
            var bytes = new byte[numberChars / 2];
            for (var i = 0; i < numberChars; i += 2)
            {
                bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
            }
            return bytes;
        }
    }
}

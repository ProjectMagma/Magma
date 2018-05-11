using System;
using System.Runtime.InteropServices;
using Xunit;

namespace Magma.Common.Facts
{
    public class Crc32Facts
    {
        static uint[] crc_table =
            {
    0x4DBDF21C, 0x500AE278, 0x76D3D2D4, 0x6B64C2B0,
    0x3B61B38C, 0x26D6A3E8, 0x000F9344, 0x1DB88320,
    0xA005713C, 0xBDB26158, 0x9B6B51F4, 0x86DC4190,
    0xD6D930AC, 0xCB6E20C8, 0xEDB71064, 0xF0000000
  };

        static uint[] crc_table2 = new uint[256];

        private static void calcTable()
        {
            for (var i = 0; i <= 255; i++)
            {
                var crc = (uint)i;
                for (var j = 7; j >= 0; j--)
                {    // Do eight times.
                    var mask = (uint)( -(crc & 1));
                    crc = (crc >> 1) ^ (0xEDB88320 & mask);
                }
                crc_table2[i] = crc;
            }
        }

        public static uint calculate(Span<byte> data)
        {
            

            var crc = 0xffffffff;

            for (var n = 0; n < data.Length; n++)
            {
                crc = (crc >> 4) ^ crc_table[(crc ^ (data[n] >> 0)) & 0x0F];  /* lower nibble */
                crc = (crc >> 4) ^ crc_table[(crc ^ (data[n] >> 4)) & 0x0F];  /* upper nibble */
            }
            
            return crc;
        }



        private byte[] _networkPacket = new byte[] { 0x00, 0x15, 0x5D, 0xF3, 0x43, 0x11, 0x82, 0x15, 0xF5, 0x19, 0xE6, 0x94, 0x08, 0x00, 0x45, 0x00, 0x00, 0x4C, 0x15, 0xDF, 0x00, 0x00, 0x80, 0x06, 0xA0, 0x69, 0xAC, 0x1E, 0x96, 0x11, 0xAC, 0x1E, 0x96, 0x15, 0xF8, 0x06, 0x00, 0x16, 0x7D, 0x73, 0x6D, 0x5B, 0xC9, 0x25, 0xAE, 0x7E, 0x50, 0x18, 0x20, 0x14, 0x27, 0x43, 0x00, 0x00, 0xC8, 0x48, 0x2D, 0xEE, 0xE4, 0x89, 0x04, 0xC5, 0x56, 0x4B, 0x04, 0x51, 0xC5, 0xC7, 0x08, 0x8D, 0x47, 0x06, 0xE6, 0x1E, 0xEF, 0xE1, 0xD8, 0xBE, 0xFB, 0xAF, 0xBD, 0x83, 0xBB, 0x36, 0xA0, 0x3B, 0x3F, 0x73, 0x37, 0x07 };

        [Fact]
        public void CalcTable()
        {
            calcTable();
            for(var i = 0; i < crc_table.Length;i++)
            {
                Assert.Equal(crc_table[i], crc_table2[i]);
            }
        }

        [Fact]
        public void ComputeCRC()
        {
            var data = _networkPacket.AsSpan().Slice(0, _networkPacket.Length - 4);
            var crcResult = calculate(data);

            var expected = MemoryMarshal.Cast<byte, uint>(_networkPacket.AsSpan().Slice(_networkPacket.Length - 4))[0];

            Assert.Equal(expected, crcResult);
        }
    }
}

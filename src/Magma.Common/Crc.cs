using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Magma.Common
{
    public class Crc
    {
        private static uint[][] _crcTable = CreateCrcTable();
        const uint POLY = 0x04c11db6;
//#define ETHER_CRC_POLY_LE	0xedb88320
//#define ETHER_CRC_POLY_BE	0x04c11db6

        private static uint[][] CreateCrcTable()
        {
            var array = new uint[8][];
            for (var i = 0; i < array.Length; i++)
            {
                array[i] = new uint[256];
            }

            uint crc;

            for (var n = 0u; n < 256; n++)
            {
                crc = n;
                crc = (crc & 1) > 0 ? (crc >> 1) ^ POLY : crc >> 1;
                crc = (crc & 1) > 0 ? (crc >> 1) ^ POLY : crc >> 1;
                crc = (crc & 1) > 0 ? (crc >> 1) ^ POLY : crc >> 1;
                crc = (crc & 1) > 0 ? (crc >> 1) ^ POLY : crc >> 1;
                crc = (crc & 1) > 0 ? (crc >> 1) ^ POLY : crc >> 1;
                crc = (crc & 1) > 0 ? (crc >> 1) ^ POLY : crc >> 1;
                crc = (crc & 1) > 0 ? (crc >> 1) ^ POLY : crc >> 1;
                crc = (crc & 1) > 0 ? (crc >> 1) ^ POLY : crc >> 1;
                array[0][n] = crc;
            }

            for (var n = 0; n < 256; n++)
            {
                crc = array[0][n];
                for (var k = 1; k < 8; k++)
                {
                    crc = array[0][crc & 0xff] ^ (crc >> 8);
                    array[k][n] = crc;
                }
            }

            return array;
        }

        public static uint Crc32(uint intiial, Span<byte> buffer)
        {
            ulong crc;

            crc = intiial ^ 0xffffffff;
            while (buffer.Length > 0 && (buffer.Length & 7) != 0)
            {
                crc = _crcTable[0][(crc ^ buffer[0]) & 0xff] ^ (crc >> 8);
                buffer = buffer.Slice(1);
            }
            while (buffer.Length >= 8)
            {
                crc ^= MemoryMarshal.Cast<byte,ulong>(buffer)[0];
                crc = _crcTable[7][crc & 0xff] ^
                      _crcTable[6][(crc >> 8) & 0xff] ^
                      _crcTable[5][(crc >> 16) & 0xff] ^
                      _crcTable[4][(crc >> 24) & 0xff] ^
                      _crcTable[3][(crc >> 32) & 0xff] ^
                      _crcTable[2][(crc >> 40) & 0xff] ^
                      _crcTable[1][(crc >> 48) & 0xff] ^
                      _crcTable[0][crc >> 56];
                buffer = buffer.Slice(8);
            }
            while (buffer.Length > 0)
            {
                crc = _crcTable[0][(crc ^ buffer[0]) & 0xff] ^ (crc >> 8);
                buffer = buffer.Slice(1);
            }
            return (uint)crc ^ 0xffffffff;
        }
    }

}

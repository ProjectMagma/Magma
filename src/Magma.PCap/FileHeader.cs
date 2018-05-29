using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Magma.PCap
{
    [StructLayout(LayoutKind.Sequential, Pack =1)]
    public struct FileHeader
    {
        private uint _magicNumber;
        private ushort _versionMajor;
        private ushort _versionMinor;
        private uint _timezone;
        private uint _sigFlags;
        public uint SnapLength;
        private NetworkKind _network;

        public static FileHeader Create()
        {
            var fileHeader = new FileHeader()
            {
                _magicNumber = 0xa1b2c3d4,
                _versionMajor = 2,
                _versionMinor = 4,
                _timezone = 0,
                _sigFlags = 0,
                SnapLength = 2000,
                _network = NetworkKind.LINKTYPE_ETHERNET,
            };
            return fileHeader;
        }
    }
}

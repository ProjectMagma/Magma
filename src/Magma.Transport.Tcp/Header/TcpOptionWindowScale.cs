using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Magma.Transport.Tcp.Header
{
    [StructLayout(LayoutKind.Sequential, Pack =1)]
    public struct TcpOptionWindowScale
    {
        private byte _padding;
        private TcpOptionKind _optionKind;
        private byte _size;
        private byte _scale;

        public TcpOptionWindowScale(byte scale)
        {
            _padding = 1;
            _optionKind = TcpOptionKind.WindowScale;
            _size = 3;
            _scale = scale;
        }
    }
}

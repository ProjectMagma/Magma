using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Magma.Transport.Tcp.Header
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct TcpOptionMaxSegmentSize
    {
        private TcpOptionKind _optionKind;
        private byte _length;
        private ushort _size;

        public TcpOptionMaxSegmentSize(ushort size)
        {
            _optionKind = TcpOptionKind.MaximumSegmentSize;
            _length = 4;
            _size = (ushort)System.Net.IPAddress.HostToNetworkOrder((short)size);
        }

        public ushort Size => (ushort)System.Net.IPAddress.NetworkToHostOrder((short)_size);
    }
}

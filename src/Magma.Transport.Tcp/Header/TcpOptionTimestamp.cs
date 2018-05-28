using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Magma.Transport.Tcp.Header
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct TcpOptionTimestamp
    {
        private ushort _padding;
        private TcpOptionKind _optionKind;
        private byte _size;
        private uint _timestampValue;
        private uint _timestampEchoReply;

        public TcpOptionTimestamp(uint timestampValue, uint timestampEchoReply)
        {
            _padding = 0x0101;
            _optionKind = TcpOptionKind.Timestamps;
            _size = 10;
            _timestampValue = (uint)System.Net.IPAddress.HostToNetworkOrder((int)timestampValue);
            _timestampEchoReply = (uint)System.Net.IPAddress.HostToNetworkOrder((int)timestampEchoReply);
        }
    }
}

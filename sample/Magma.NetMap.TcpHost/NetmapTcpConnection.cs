using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using Magma.Link;
using Magma.Network;
using Magma.Network.Header;
using Magma.Transport.Tcp;
using Magma.Transport.Tcp.Header;
using Microsoft.AspNetCore.Server.Kestrel.Transport.Abstractions.Internal;
using static Magma.Network.IPAddress;

namespace Magma.NetMap.TcpHost
{
    public class NetMapTcpConnection : TcpConnection
    {
        private TcpReceiver _tcpReceiver;
        private TransportConnection _connection;

        public NetMapTcpConnection(Ethernet ethernetHeader, IPv4 ipHeader, TcpReceiver tcpReceiver)
            : base(ethernetHeader, ipHeader)
        {
            _tcpReceiver = tcpReceiver;
            _connection = new TransportConnection();
        }

        public TransportConnection Connection => _connection;

        protected override uint GetRandomSequenceStart() => _tcpReceiver.RandomSeqeunceNumber();
        protected override uint GetTimestamp() => (uint)DateTime.UtcNow.TimeOfDay.TotalMilliseconds;

        protected override void WriteMemory(Memory<byte> memory)
        {
            _tcpReceiver.Transmitter.SendBuffer(memory);
            _tcpReceiver.Transmitter.ForceFlush();
        }

        protected override bool TryGetMemory(out Memory<byte> memory) => _tcpReceiver.Transmitter.TryGetNextBuffer(out memory);
    }
}

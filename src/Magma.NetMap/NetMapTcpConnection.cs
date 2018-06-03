using System;
using System.Buffers;
using System.Collections.Generic;
using System.Text;
using Magma.Network.Header;
using Magma.Transport.Tcp;
using Microsoft.AspNetCore.Server.Kestrel.Transport.Abstractions.Internal;

namespace Magma.NetMap
{
    internal sealed class NetMapTcpConnection : TcpConnection
    {
        private NetMapTransportReceiver _tcpReceiver;
        
        public NetMapTcpConnection(Ethernet ethernetHeader, IPv4 ipHeader, Tcp tcpHeader,
            NetMapTransportReceiver tcpReceiver, IConnectionDispatcher connectionDispatcher)
            : base(ethernetHeader, ipHeader, tcpHeader, System.IO.Pipelines.PipeScheduler.ThreadPool, 
                  System.IO.Pipelines.PipeScheduler.ThreadPool, MemoryPool<byte>.Shared, connectionDispatcher)
        {
            _tcpReceiver = tcpReceiver;
        }

        protected override uint GetRandomSequenceStart() => _tcpReceiver.RandomSeqeunceNumber();
        protected override uint GetTimestamp() => (uint)DateTime.UtcNow.TimeOfDay.TotalMilliseconds;

        protected override void WriteMemory(Memory<byte> memory)
        {
            _tcpReceiver.Transmitter.SendBuffer(memory);
        }

        protected override bool TryGetMemory(out Memory<byte> memory) => _tcpReceiver.Transmitter.TryGetNextBuffer(out memory);
    }
}

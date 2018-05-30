using System;
using System.Buffers;
using System.Collections.Generic;
using System.Text;
using Magma.Network.Header;
using Magma.Transport.Tcp;

namespace Magma.NetMap
{
    internal class NetMapTcpConnection : TcpConnection
    {
        private NetMapTransportReceiver _tcpReceiver;
        private PCap.PCapFileWriter _writer;

        public NetMapTcpConnection(Ethernet ethernetHeader, IPv4 ipHeader, Tcp tcpHeader, NetMapTransportReceiver tcpReceiver, PCap.PCapFileWriter writer)
            : base(ethernetHeader, ipHeader, tcpHeader, System.IO.Pipelines.PipeScheduler.ThreadPool, System.IO.Pipelines.PipeScheduler.ThreadPool, MemoryPool<byte>.Shared)
        {
            _tcpReceiver = tcpReceiver;
        }

        protected override uint GetRandomSequenceStart() => _tcpReceiver.RandomSeqeunceNumber();
        protected override uint GetTimestamp() => (uint)DateTime.UtcNow.TimeOfDay.TotalMilliseconds;

        protected override void WriteMemory(Memory<byte> memory)
        {
            _writer.WritePacket(memory.Span);
            _tcpReceiver.Transmitter.SendBuffer(memory);
            _tcpReceiver.Transmitter.ForceFlush();
        }

        protected override bool TryGetMemory(out Memory<byte> memory) => _tcpReceiver.Transmitter.TryGetNextBuffer(out memory);

        
    }
}

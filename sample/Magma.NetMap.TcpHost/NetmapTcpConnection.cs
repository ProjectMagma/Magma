using System;
using System.Buffers;
using Magma.Network.Header;
using Magma.Transport.Tcp;

namespace Magma.NetMap.TcpHost
{
    public class NetMapTcpConnection : TcpConnection
    {
        private TcpReceiver _tcpReceiver;

        public NetMapTcpConnection(Ethernet ethernetHeader, IPv4 ipHeader, TcpReceiver tcpReceiver)
            : base(ethernetHeader, ipHeader, System.IO.Pipelines.PipeScheduler.ThreadPool, System.IO.Pipelines.PipeScheduler.ThreadPool, MemoryPool<byte>.Shared)
        {
            _tcpReceiver = tcpReceiver;

        }

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

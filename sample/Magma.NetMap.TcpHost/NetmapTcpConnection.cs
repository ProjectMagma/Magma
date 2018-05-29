using System;
using System.Buffers;
using Magma.Network.Header;
using Magma.Transport.Tcp;

namespace Magma.NetMap.TcpHost
{
    public class NetMapTcpConnection : TcpConnection
    {
        private TcpReceiver _tcpReceiver;
        private PCap.PCapFileWriter _writer;
        public NetMapTcpConnection(Ethernet ethernetHeader, IPv4 ipHeader, Tcp tcpHeader, TcpReceiver tcpReceiver, PCap.PCapFileWriter writer)
            : base(ethernetHeader, ipHeader,tcpHeader, System.IO.Pipelines.PipeScheduler.ThreadPool, System.IO.Pipelines.PipeScheduler.ThreadPool, MemoryPool<byte>.Shared)
        {
            _tcpReceiver = tcpReceiver;
            _writer = writer;
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

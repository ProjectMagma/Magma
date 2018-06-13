using System;
using System.Buffers;
using System.Collections.Generic;
using System.Text;
using Magma.Network.Header;
using Magma.Transport.Tcp;
using Magma.Transport.Tcp.Header;
using Microsoft.AspNetCore.Server.Kestrel.Transport.Abstractions.Internal;
using Xunit;

namespace Magma.Internet.Ip.Facts
{
    public class TcpConnectionFacts
    {
        //private static readonly byte[] s_synPacket = "74 ea 3a 38 f1 00 00 19 d2 90 29 61 08 00 45 00 00 3c 20 e3 40 00 40 06 d9 2d c0 a8 01 68 d8 12 a6 88 c2 c3 00 50 d5 e2 df b4 00 00 00 00 a0 02 20 00 c4 47 00 00 02 04 05 b4 01 03 03 02 04 02 08 0a 00 04 aa 62 00 00 00 00".HexToByteArray();
        //private static readonly byte[] s_synAckPacket =  "00 19 d2 90 29 61 74 ea 3a 38 f1 00 08 00 45 00 00 3c 00 00 40 00 2d 06 0d 11 d8 12 a6 88 c0 a8 01 68 00 50 c2 c3 29 91 a6 b8 d5 e2 df b5 a0 12 16 a0 f8 e7 00 00 02 04 05 a0 01 01 08 0a 4e 62 b9 10 00 04 aa 62 01 03 03 09".HexToByteArray();
        //private static readonly byte[] s_ackPacket = "74 ea 3a 38 f1 00 00 19 d2 90 29 61 08 00 45 00 00 34 20 e4 40 00 40 06 d9 34 c0 a8 01 68 d8 12 a6 88 c2 c3 00 50 d5 e2 df b5 29 91 a6 b9 80 10 10 bc 2a 66 00 00 01 01 08 0a 00 04 aa 81 4e 62 b9 10".HexToByteArray();

        //[Fact]
        //public void ThreeWayHandshakeWorks()
        //{
        //    var synSpan = s_synPacket.AsSpan();
        //    var synAckSpan = s_synAckPacket.AsSpan();

        //    Assert.True(Ethernet.TryConsume(synAckSpan, out var synAckEthHeader, out var data));
        //    Assert.True(IPv4.TryConsume(data, out var synAckIpHeader, out data));
        //    Assert.True(TcpHeaderWithOptions.TryConsume(data, out var synAckTcpHeader, out data));

        //    Assert.True(Ethernet.TryConsume(synSpan, out var etherHeader, out data));
        //    Assert.True(IPv4.TryConsume(data, out var ipHeader, out data, true));
        //    Assert.True(TcpHeaderWithOptions.TryConsume(data, out var tcpHeader, out data));

        //    var connection = new TestTcpConnection(etherHeader, ipHeader, tcpHeader.Header, null);
        //    connection.ProcessPacket(tcpHeader, data);
        //}


        //private class TestTcpConnection : TcpConnection
        //{
        //    public TestTcpConnection(Ethernet etherHeader, IPv4 ipHeader, Tcp tcpHeader, IConnectionDispatcher connectionDisptacher)
        //        : base(etherHeader, ipHeader, tcpHeader, System.IO.Pipelines.PipeScheduler.ThreadPool, System.IO.Pipelines.PipeScheduler.ThreadPool, MemoryPool<byte>.Shared, connectionDisptacher)
        //    {
        //    }

        //    protected override uint GetRandomSequenceStart() => 697411256;

        //    protected override uint GetTimestamp() => 1315092752;

        //    protected override bool TryGetMemory(out Memory<byte> memory)
        //    {
        //        memory = (new byte[2048]).AsMemory();
        //        return true;
        //    }

        //    protected override void WriteMemory(Memory<byte> memory)
        //    {
        //        for(var i = 0; i < memory.Length;i++)
        //        {
        //            Assert.Equal(s_synAckPacket[i], memory.Span[i]);
        //        }
        //    }
        //}
    }
}

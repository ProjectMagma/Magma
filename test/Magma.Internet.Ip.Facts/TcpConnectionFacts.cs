using System;
using System.Collections.Generic;
using System.Text;
using Magma.Network.Header;
using Magma.Transport.Tcp;
using Magma.Transport.Tcp.Header;
using Xunit;

namespace Magma.Internet.Ip.Facts
{
    public class TcpConnectionFacts
    {
        private static readonly byte[] s_synPacket =  "00 19 d2 90 29 61 74 ea 3a 38 f1 00 08 00 45 00 00 3c 00 00 40 00 2d 06 0d 11 d8 12 a6 88 c0 a8 01 68 00 50 c2 c3 29 91 a6 b8 d5 e2 df b5 a0 12 16 a0 f8 e7 00 00 02 04 05 a0 01 01 08 0a 4e 62 b9 10 00 04 aa 62 01 03 03 09".HexToByteArray();
        
        [Fact]
        public void ThreeWayHandshakeWorks()
        {
            var span = s_synPacket.AsSpan();

            Assert.True(Ethernet.TryConsume(span, out var etherHeader, out var data));
            Assert.True(IPv4.TryConsume(data, out var ipHeader, out data, true));
            Assert.True(TcpHeaderWithOptions.TryConsume(data, out var tcpHeader, out data));

            var connection = new TestTcpConnection(etherHeader, ipHeader);
            connection.ProcessPacket(tcpHeader, data);
        }


        private class TestTcpConnection : TcpConnection
        {
            public TestTcpConnection(Ethernet etherHeader, IPv4 ipHeader)
                :base(etherHeader, ipHeader)
            {
            }

            protected override uint GetRandomSequenceStart()
            {
                throw new NotImplementedException();
            }

            protected override bool TryGetMemory(out Memory<byte> memory)
            {
                throw new NotImplementedException();
            }

            protected override void WriteMemory(Memory<byte> memory)
            {
                throw new NotImplementedException();
            }
        }
    }
}

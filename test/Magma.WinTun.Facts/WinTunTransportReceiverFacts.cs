using System;
using System.Buffers;
using Magma.WinTun;
using Xunit;

namespace Magma.WinTun.Facts
{
    public class WinTunTransportReceiverFacts
    {
        [Fact]
        public void CanInstantiateWinTunTransportReceiver()
        {
            var receiver = new WinTunTransportReceiver();
            Assert.NotNull(receiver);
        }

        [Fact]
        public void FlushPendingAcksThrowsNotImplementedException()
        {
            var receiver = new WinTunTransportReceiver();
            Assert.Throws<NotImplementedException>(() => receiver.FlushPendingAcks());
        }

        [Fact]
        public void TryConsumeThrowsNotImplementedException()
        {
            var receiver = new WinTunTransportReceiver();
            var memory = MemoryPool<byte>.Shared.Rent(100);
            
            Assert.Throws<NotImplementedException>(() => receiver.TryConsume(memory));
        }
    }
}

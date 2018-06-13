using System;
using System.Buffers;
using System.Collections.Generic;
using System.Text;
using Magma.Network.Abstractions;

namespace Magma.WinTun
{
    public class WinTunTransportReceiver : IPacketReceiver
    {
        public void FlushPendingAcks()
        {
            throw new NotImplementedException();
        }

        public T TryConsume<T>(T input) where T : IMemoryOwner<byte>
        {
            throw new NotImplementedException();
        }
    }
}

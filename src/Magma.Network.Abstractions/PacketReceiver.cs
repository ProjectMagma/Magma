using System;
using System.Buffers;

namespace Magma.Network.Abstractions
{
    public interface IPacketReceiver
    {
        T TryConsume<T>(T input) where T : IMemoryOwner<byte>;
        void FlushPendingAcks();
    }
}

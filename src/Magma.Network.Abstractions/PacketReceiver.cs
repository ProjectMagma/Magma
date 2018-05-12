using System;
using System.Buffers;

namespace Magma.Network.Abstractions
{
    public interface IPacketReceiver
    {
        bool TryConsume<T>(T input) where T : IMemoryOwner<byte>;
    }
}

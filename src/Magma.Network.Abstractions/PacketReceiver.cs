using System;
using System.Buffers;

namespace Magma.Network.Abstractions
{
    public interface IPacketReceiver
    {
        T TryConsume<T>(T buffer) where T : struct, IMemoryOwner<byte>;
    }
}

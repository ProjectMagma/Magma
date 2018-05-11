using System;

namespace Magma.Network.Abstractions
{
    public interface IPacketReceiver
    {
        bool TryConsume(ReadOnlySpan<byte> input);
    }
}

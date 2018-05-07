using System;

namespace Magma.Network.Abstractions
{
    public interface IPacketReceiver
    {
        bool TryConsume(int ringId, Span<byte> buffer);
    }
}

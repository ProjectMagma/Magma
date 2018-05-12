using System;
using System.Buffers;
using Magma.Network.Abstractions;

namespace Magma.Network
{
    public struct PacketReceiver : IPacketReceiver
    {
        public T TryConsume<T>(T memoryOwner) where T : struct, IMemoryOwner<byte> => memoryOwner;
    }
}

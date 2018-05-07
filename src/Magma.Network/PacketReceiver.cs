using System;
using Magma.Network.Abstractions;

namespace Magma.Network
{
    public struct PacketReceiver : IPacketReceiver
    {
        public bool TryConsume(int ringId, Span<byte> buffer)
        {
            Console.WriteLine($"Received packet on ring {ringId} size was {buffer.Length}");
            return false;
        }
    }
}

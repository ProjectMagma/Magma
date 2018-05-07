using System;
using Magma.NetMap.Interop;
using Magma.Network.Abstractions;

namespace Magma.NetMap.Host
{
    struct PacketReceiver : IPacketReceiver
    {
        public bool TryConsume(int ringId, Span<byte> buffer)
        {
            Console.WriteLine($"Received packet on ring {ringId} size was {buffer.Length}");
            return false;
        }
    }

    class Program
    {
        static unsafe void Main(string[] args)
        {
            var netmap = new NetMapPort<PacketReceiver>("eth0", () => new PacketReceiver());
            netmap.Open();
            netmap.PrintPortInfo();

            Console.WriteLine("Started reading");
            Console.ReadLine();
        }
    }
}

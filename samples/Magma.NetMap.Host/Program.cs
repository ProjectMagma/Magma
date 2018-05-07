using System;
using System.IO;
using Magma.NetMap.Interop;
using Magma.Network.Abstractions;

namespace Magma.NetMap.Host
{
    class PacketReceiver : IPacketReceiver
    {
        private int _ringId;
        private StreamWriter _streamWriter;

        public PacketReceiver(int ringId)
        {
            _streamWriter = new StreamWriter($"rxOutput{ringId}.txt");
            _ringId = ringId;
        }
        
        public bool TryConsume(int ringId, Span<byte> buffer)
        {
            _streamWriter.WriteLine("------------------------");
            _streamWriter.WriteLine(BitConverter.ToString(buffer.ToArray()));
            _streamWriter.Flush();
            return false;
        }
    }
    
    class Program
    {
        private static int RingId = 0;

        static unsafe void Main(string[] args)
        {
            var interfaceName = args?[0] ?? "eth0";
            var netmap = new NetMapPort<PacketReceiver>( "eth0", () => new PacketReceiver(RingId++));
            netmap.Open();
            netmap.PrintPortInfo();

            Console.WriteLine("Started reading");
            Console.ReadLine();
        }
    }
}

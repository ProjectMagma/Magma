using System;
using System.IO;
using Magma.NetMap.Interop;
using Magma.Network.Abstractions;
using Magma.Network.Header;

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
            if (Ethernet.TryConsume(ref buffer, out var ethernet))
            {
                _streamWriter.WriteLine($"{ethernet.ToString()}");
            }
            else
            {
                _streamWriter.WriteLine("---> Unknown");
            }
            
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
            var interfaceName = "eth0";
            if (args.Length >= 1)
            {
                interfaceName = args[0];
            }

            var netmap = new NetMapPort<PacketReceiver>(interfaceName, () => new PacketReceiver(RingId++));
            netmap.Open();
            netmap.PrintPortInfo();

            Console.WriteLine("Started reading");
            Console.ReadLine();
        }
    }
}

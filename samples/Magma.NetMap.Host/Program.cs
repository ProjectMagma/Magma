using System;
using System.IO;
using Magma.Internet.Icmp;
using Magma.Internet.Ip;
using Magma.Network;
using Magma.Network.Abstractions;
using Magma.Network.Header;

namespace Magma.NetMap.Host
{
    class PacketReceiver : IPacketReceiver
    {
        private int _ringId;
        private NetMapTransmitRing _transmitter;
        private TextWriter _streamWriter;

        public PacketReceiver(int ringId, NetMapTransmitRing transmitter, bool logToFile)
        {
            _transmitter = transmitter;
            var filename = Path.Combine(Directory.GetCurrentDirectory(), $"rxOutput{ringId}.txt");
            Console.WriteLine($"Outputing recieved packets to: {filename}");
            _streamWriter = logToFile ? new StreamWriter(filename) : null;
            _ringId = ringId;
        }
        
        public bool TryConsume(int ringId, Span<byte> buffer)
        {
            if (Ethernet.TryConsume(ref buffer, out var ethernet))
            {
                _streamWriter?.WriteLine($"{ethernet.ToString()}");

                if (ethernet.Ethertype == EtherType.IPv4)
                {
                    if (IPv4.TryConsume(ref buffer, out var ip))
                    {
                        _streamWriter?.WriteLine($"{ip.ToString()}");

                        var protocol = ip.Protocol;
                        if (protocol == ProtocolNumber.Tcp)
                        {
                            if (Tcp.TryConsume(ref buffer, out var tcp))
                            {
                                _streamWriter?.WriteLine($"{tcp.ToString()}");
                            }
                        }
                        else if (protocol == ProtocolNumber.Icmp)
                        {
                            if (IcmpV4.TryConsume(ref buffer, out var icmp))
                            {
                                _streamWriter?.WriteLine($"{icmp.ToString()}");

                                if (icmp.Code == Code.EchoRequest)
                                {
                                    // Need to transmit here...
                                }
                            }
                        }
                    }
                }
                else
                {
                    _streamWriter?.WriteLine($"{ ethernet.Ethertype.ToString().PadRight(11)} ---> {BitConverter.ToString(buffer.ToArray()).Substring(60)}...");
                }
                _streamWriter?.WriteLine("+--------------------------------------------------------------------------------------+" + Environment.NewLine);
            }
            else
            {
                _streamWriter?.WriteLine($"Unknown ---> {BitConverter.ToString(buffer.ToArray()).Substring(60)}...");
            }
            
            _streamWriter?.Flush();
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

            var netmap = new NetMapPort<PacketReceiver>(interfaceName, transmitter => new PacketReceiver(RingId++, transmitter, logToFile : true));
            netmap.Open();
            netmap.PrintPortInfo();

            Console.WriteLine("Started reading");
            Console.ReadLine();
        }
    }
}

using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
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

        public PacketReceiver(int ringId, NetMapTransmitRing transmitter, bool log, bool loggingToFile)
        {
            _transmitter = transmitter;
            var filename = Path.Combine(Directory.GetCurrentDirectory(), $"rxOutput{ringId}.txt");
            Console.WriteLine($"Outputing recieved packets to: {filename}");
            if (log)
            {
                if (loggingToFile)
                {
                    _streamWriter = new StreamWriter(filename);
                }
                else
                {
                    _streamWriter = new StreamWriter(Console.OpenStandardOutput());
                }
            }

            _ringId = ringId;
        }

        public bool TryConsume(int ringId, Span<byte> input)
        {
            var data = input;
            if (Ethernet.TryConsume(ref data, out var ether))
            {
                WriteLine($"{ether.ToString()}");

                if (ether.Ethertype == EtherType.IPv4)
                {
                    if (IPv4.TryConsume(ref data, out var ip))
                    {
                        WriteLine($"{ip.ToString()}");

                        if (!ip.IsChecksumValid())
                        {
                            // Consume packets with invalid checksums; but don't do further processing
                            return true;
                        }

                        var protocol = ip.Protocol;
                        if (protocol == ProtocolNumber.Tcp)
                        {
                            if (Tcp.TryConsume(ref data, out var tcp))
                            {
                                WriteLine($"{tcp.ToString()}");
                            }
                        }
                        else if (protocol == ProtocolNumber.Icmp)
                        {
                            if (IcmpV4.TryConsume(ref data, out var icmp))
                            {
                                WriteLine($"{icmp.ToString()}");

                                if (icmp.Code == Code.EchoRequest)
                                {
                                    if (_transmitter.TryGetNextBuffer(out var output))
                                    {
                                        var span = output.Span;
                                        input.CopyTo(span);

                                        // Swap destinations
                                        ref byte current = ref MemoryMarshal.GetReference(span);
                                        ref var etherOut = ref Unsafe.As<byte, Ethernet>(ref current);
                                        var srcMac = etherOut.Source;
                                        etherOut.Source = etherOut.Destination;
                                        etherOut.Destination = srcMac;

                                        current = ref Unsafe.Add(ref current, Unsafe.SizeOf<Ethernet>());

                                        ref var ipOuput = ref Unsafe.As<byte, IPv4>(ref current);
                                        var srcIp = ipOuput.SourceAddress;
                                        ipOuput.SourceAddress = ipOuput.DestinationAddress;
                                        ipOuput.DestinationAddress = srcIp;
                                        ipOuput.HeaderChecksum = 0;
                                        ipOuput.HeaderChecksum = Checksum.Calcuate(in ipOuput, Unsafe.SizeOf<IPv4>());

                                        current = ref Unsafe.Add(ref current, Unsafe.SizeOf<IPv4>());

                                        ref var icmpOutput = ref Unsafe.As<byte, IcmpV4>(ref current);
                                        icmpOutput.Code = Code.EchoReply;
                                        icmpOutput.HeaderChecksum = 0;
                                        icmpOutput.HeaderChecksum = Checksum.Calcuate(in icmpOutput, Unsafe.SizeOf<IcmpV4>());

                                        _transmitter.SendBuffer(output.Slice(0, input.Length));
                                        WriteLine($"SENT { ether.Ethertype.ToString().PadRight(11)} ---> {BitConverter.ToString(output.Slice(0,input.Length).ToArray()).Substring(60)}...");
                                        _transmitter.ForceFlush();
                                        return true;
                                    }
                                }
                            }
                        }
                    }
                }
                else
                {
                    WriteLine($"{ ether.Ethertype.ToString().PadRight(11)} ---> {BitConverter.ToString(data.ToArray()).Substring(60)}...");
                }
                WriteLine("+--------------------------------------------------------------------------------------+" + Environment.NewLine);
            }
            else
            {
                WriteLine($"Unknown ---> {BitConverter.ToString(data.ToArray()).Substring(60)}...");
            }

            Flush();
            return false;
        }

        private void WriteLine(string output) => _streamWriter?.WriteLine(output);
        private void Flush() => _streamWriter?.Flush();
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

            Console.WriteLine($"Ethernet Header length: {Unsafe.SizeOf<Ethernet>()}");
            Console.WriteLine($"IP Header length: {Unsafe.SizeOf<IPv4>()}");
            Console.WriteLine($"TCP Header length: {Unsafe.SizeOf<Tcp>()}");

            var netmap = new NetMapPort<PacketReceiver>(interfaceName, transmitter => new PacketReceiver(RingId++, transmitter, log: true, loggingToFile: false));
            netmap.Open();
            netmap.PrintPortInfo();

            Console.WriteLine("Started reading");
            while (true)
            {
                var line = Console.ReadLine();
                if (line == "x") return;
                if (!netmap.TransmitRings[0].TryGetNextBuffer(out var buffer)) throw new Exception("Failed to get buffer");
                netmap.TransmitRings[0].SendBuffer(buffer);
                Console.WriteLine("Sent buffer!");
            }
        }
    }
}

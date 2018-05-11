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
            if (log)
            {
                if (loggingToFile)
                {
                    var filename = Path.Combine(Directory.GetCurrentDirectory(), $"rxOutput{ringId}.txt");
                    Console.WriteLine($"Outputing recieved packets to: {filename}");
                    _streamWriter = new StreamWriter(filename);
                }
                else
                {
                    _streamWriter = new StreamWriter(Console.OpenStandardOutput());
                }
            }

            _ringId = ringId;
        }

        public unsafe bool TryConsume(ReadOnlySpan<byte> input)
        {
            bool result;
            if (Ethernet.TryConsume(input, out var etherIn, out var data))
            {
                WriteLine($"{etherIn.ToString()}");

                if (etherIn.Ethertype == EtherType.IPv4)
                {
                    result = TryConsumeIPv4(in etherIn, data);
                }
                else
                {
                    WriteLine($"{ etherIn.Ethertype.ToString().PadRight(11)} ---> {BitConverter.ToString(data.ToArray()).MaxLength(60)}");
                    // Pass on to host
                    result = false;
                }
            }
            else
            {
                WriteLine($"Ether not parsed ---> {BitConverter.ToString(data.ToArray()).MaxLength(60)}");
                // Consume invalid IP packets and don't do further processing
                result = true;
            }
            WriteLine("+--------------------------------------------------------------------------------------+" + Environment.NewLine);

            Flush();
            return result;
        }

        public unsafe bool TryConsumeIPv4(in Ethernet ethernetFrame, ReadOnlySpan<byte> input)
        {
            if (IPv4.TryConsume(input, out var ipIn, out var data))
            {
                WriteLine($"{ipIn.ToString()}");

                var protocol = ipIn.Protocol;
                if (protocol == ProtocolNumber.Tcp)
                {
                    return TryConsumeTcp(in ethernetFrame, in ipIn, data);
                }
                else if (protocol == ProtocolNumber.Icmp)
                {
                    return TryConsumeIcmp(in ethernetFrame, in ipIn, data);
                }
                else
                {
                    WriteLine($"Other protocol {protocol.ToString()} ---> {BitConverter.ToString(data.ToArray()).MaxLength(60)}");
                    // Pass to host
                    return false;
                }
            }
            else
            {
                // Couldn't parse; consume the invalid packet
                return true;
            }
        }

        public unsafe bool TryConsumeTcp(in Ethernet ethernetFrame, in IPv4 ipv4, ReadOnlySpan<byte> input)
        {
            if (Tcp.TryConsume(input, out var tcpIn, out var data))
            {
                WriteLine($"{tcpIn.ToString()}");
                // Pass to host to deal with
                return false;
            }
            else
            {
                WriteLine($"TCP not parsed ---> {BitConverter.ToString(input.ToArray()).MaxLength(60)}");
                // Couldn't parse; consume the invalid packet
                return true;
            }
        }

        public unsafe bool TryConsumeIcmp(in Ethernet ethernetFrame, in IPv4 ipv4, ReadOnlySpan<byte> input)
        {
            if (!IcmpV4.IsChecksumValid(ref MemoryMarshal.GetReference(input), ipv4.DataLength))
            {
                WriteLine($"In Icmp (Checksum Invalid) -> {BitConverter.ToString(input.ToArray())}");
                // Consume packets with invalid checksums; but don't do further processing
                return true;
            }

            if (IcmpV4.TryConsume(input, out var icmpIn, out var data))
            {
                WriteLine($"{icmpIn.ToString()}");

                if (icmpIn.Code == Code.EchoRequest)
                {
                    if (_transmitter.TryGetNextBuffer(out var txMemory))
                    {
                        var output = txMemory.Span;

                        ref byte current = ref MemoryMarshal.GetReference(output);
                        ref var etherOut = ref Unsafe.As<byte, Ethernet>(ref current);
                        // Swap source & destination
                        etherOut.Destination = ethernetFrame.Source;
                        etherOut.Source = ethernetFrame.Destination;
                        etherOut.Ethertype = ethernetFrame.Ethertype;

                        current = ref Unsafe.Add(ref current, Unsafe.SizeOf<Ethernet>());

                        ref var ipOuput = ref Unsafe.As<byte, IPv4>(ref current);
                        ipOuput = ipv4;
                        // Swap source & destination
                        ipOuput.SourceAddress = ipv4.DestinationAddress;
                        ipOuput.DestinationAddress = ipv4.SourceAddress;
                        // Zero checksum and calcaulate
                        ipOuput.HeaderChecksum = 0;
                        ipOuput.HeaderChecksum = Checksum.Calcuate(ref current, Unsafe.SizeOf<IPv4>());

                        current = ref Unsafe.Add(ref current, Unsafe.SizeOf<IPv4>());

                        ref var icmpOutput = ref Unsafe.As<byte, IcmpV4>(ref current);
                        icmpOutput.Code = Code.EchoReply;

                        current = ref Unsafe.Add(ref current, Unsafe.SizeOf<IcmpV4>());
                        // Copy input data to output
                        data.CopyTo(MemoryMarshal.CreateSpan(ref current, data.Length));
                        // Zero checksum and calcaulate
                        icmpOutput.HeaderChecksum = 0;
                        icmpOutput.HeaderChecksum = Checksum.Calcuate(ref current, ipv4.DataLength);

                        if (!IcmpV4.IsChecksumValid(ref Unsafe.As<IcmpV4, byte>(ref icmpOutput), ipv4.DataLength))
                        {
                            WriteLine($"Out Icmp (Checksum Invalid) -> {BitConverter.ToString(MemoryMarshal.AsBytes(MemoryMarshal.CreateSpan(ref icmpOutput, ipv4.DataLength)).ToArray())}");
                        }

                        _transmitter.SendBuffer(txMemory.Slice(0, input.Length));
                        _transmitter.ForceFlush();
                    }
                    else
                    {
                        WriteLine($"TryGetNextBuffer returned false");
                    }
                    return true;
                }
                else
                {
                    // Pass other types onto host
                    return false;
                }
            }
            else
            {
                WriteLine($"IcmpV4 not parsed ---> {BitConverter.ToString(data.ToArray()).MaxLength(60)}");
                // Consume invalid packets; and don't do further processing
                return true;
            }
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

    public static class StringExtensions
    {
        public static string MaxLength(this string s, int length)
        {
            if (s.Length <= length)
            {
                return s;
            }

            return s.Substring(0, length - 3) + "...";
           
        }
    }
}

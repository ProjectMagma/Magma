using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Magma.Internet.Ip;
using Magma.Network.Header;

namespace Magma.NetMap.UdpEchoServer
{
    class Program
    {
        static void Main(string[] args)
        {
            var listenPort = (ushort)7777;

            if (args.Length >= 1)
            {
                if (ushort.TryParse(args[0], out var port))
                {
                    listenPort = port;
                }
            }

            Console.WriteLine("=== Magma UDP Echo Server Sample ===");
            Console.WriteLine($"Listen Port: {listenPort}");
            Console.WriteLine($"Ethernet Header: {Unsafe.SizeOf<Ethernet>()} bytes");
            Console.WriteLine($"IPv4 Header: {Unsafe.SizeOf<IPv4>()} bytes");
            Console.WriteLine($"UDP Header: {Unsafe.SizeOf<Udp>()} bytes");
            Console.WriteLine();
            Console.WriteLine("This sample demonstrates:");
            Console.WriteLine("  - UDP header parsing using Magma.Transport.Udp");
            Console.WriteLine("  - End-to-end packet processing (Ethernet -> IPv4 -> UDP)");
            Console.WriteLine("  - Zero-copy packet manipulation with Span<byte>");
            Console.WriteLine();
            Console.WriteLine("To run this sample with NetMap:");
            Console.WriteLine("  1. Ensure NetMap kernel module is loaded");
            Console.WriteLine("  2. Run with sudo or appropriate capabilities");
            Console.WriteLine("  3. Specify network interface: dotnet run -- eth0");
            Console.WriteLine();
            Console.WriteLine("Example UDP packet processing:");
            Console.WriteLine();

            DemonstrateUdpParsing(listenPort);

            Console.WriteLine();
            Console.WriteLine("Sample completed. See README.md for integration with NetMap.");
        }

        static unsafe void DemonstrateUdpParsing(ushort listenPort)
        {
            var packet = CreateSampleUdpPacket(listenPort);

            Console.WriteLine($"Sample packet ({packet.Length} bytes):");
            Console.WriteLine(BitConverter.ToString(packet));
            Console.WriteLine();

            if (Ethernet.TryConsume(packet, out var ethernet, out var etherData))
            {
                Console.WriteLine(ethernet.ToString());

                if (ethernet.Ethertype == EtherType.IPv4)
                {
                    if (IPv4.TryConsume(etherData, out var ipv4, out var ipData))
                    {
                        Console.WriteLine(ipv4.ToString());

                        if (ipv4.Protocol == ProtocolNumber.Udp)
                        {
                            if (Udp.TryConsume(ipData, out var udp, out var udpData))
                            {
                                Console.WriteLine(udp.ToString());
                                Console.WriteLine($"+- UDP Data -------------------------------------------------------------------+");
                                Console.WriteLine($"| Length: {udpData.Length} bytes".PadRight(87) + "|");
                                if (udpData.Length > 0)
                                {
                                    var preview = udpData.Length > 60 ? udpData.Slice(0, 60) : udpData;
                                    Console.WriteLine($"| {BitConverter.ToString(preview.ToArray())}".PadRight(87) + "|");
                                }
                                Console.WriteLine($"+------------------------------------------------------------------------------+");
                                Console.WriteLine();
                                Console.WriteLine($"✓ Successfully parsed UDP packet to port {udp.DestinationPort}");
                            }
                        }
                    }
                }
            }
        }

        static byte[] CreateSampleUdpPacket(ushort destPort)
        {
            var packet = new byte[64];
            var span = packet.AsSpan();

            ref byte current = ref MemoryMarshal.GetReference(span);

            ref var ethernet = ref Unsafe.As<byte, Ethernet>(ref current);
            ethernet.Destination = new MacAddress(0x00, 0x11, 0x22, 0x33, 0x44, 0x55);
            ethernet.Source = new MacAddress(0xAA, 0xBB, 0xCC, 0xDD, 0xEE, 0xFF);
            ethernet.Ethertype = EtherType.IPv4;

            current = ref Unsafe.Add(ref current, Unsafe.SizeOf<Ethernet>());

            ref var ipv4 = ref Unsafe.As<byte, IPv4>(ref current);
            ipv4.VersionAndHeaderLength = 0x45;
            ipv4.TypeOfService = 0;
            ipv4.TotalLength = (ushort)(20 + 8 + 10);
            ipv4.Identification = 12345;
            ipv4.FlagsAndFragmentOffset = 0;
            ipv4.TimeToLive = 64;
            ipv4.Protocol = ProtocolNumber.Udp;
            ipv4.SourceAddress = new IPv4Address(192, 168, 1, 100);
            ipv4.DestinationAddress = new IPv4Address(192, 168, 1, 1);
            ipv4.HeaderChecksum = 0;

            current = ref Unsafe.Add(ref current, Unsafe.SizeOf<IPv4>());

            ref var udp = ref Unsafe.As<byte, Udp>(ref current);
            udp.SourcePort = 12345;
            udp.DestinationPort = destPort;
            udp.Length = (ushort)(8 + 10);
            udp.Checksum = 0;

            current = ref Unsafe.Add(ref current, Unsafe.SizeOf<Udp>());

            var data = "Hello UDP!"u8.ToArray();
            data.AsSpan().CopyTo(MemoryMarshal.CreateSpan(ref current, data.Length));

            return packet;
        }
    }
}

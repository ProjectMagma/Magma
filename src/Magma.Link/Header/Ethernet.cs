using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Magma.Link;

namespace Magma.Network.Header
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Ethernet
    {
        public MacAddress Destination;
        public MacAddress Source;
        public EtherType Ethertype;

        public static bool TryConsume(ReadOnlySpan<byte> input, out Ethernet ethernetFrame, out ReadOnlySpan<byte> data)
        {
            if (input.Length >= Unsafe.SizeOf<Ethernet>())
            {
                ethernetFrame = Unsafe.As<byte, Ethernet>(ref MemoryMarshal.GetReference(input));
                data = input.Slice(Unsafe.SizeOf<Ethernet>(), input.Length - (Unsafe.SizeOf<Ethernet>()));
                return true; 
            }

            ethernetFrame = default;
            data = default;
            return false;
        }

        public override string ToString()
        {
            return "+- Ethernet Frame ---------------------------------------------------------------------+" + Environment.NewLine +
                  $"| {Ethertype.ToString().PadRight(11)} | {Source.ToString()} -> {Destination.ToString()}".PadRight(87) + "|";
        }
    }
}

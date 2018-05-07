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

        public static bool TryConsume(ref Span<byte> span, out Ethernet ethernet)
        {
            const int CrcSize = 4;

            if (span.Length >= Unsafe.SizeOf<Ethernet>() + CrcSize)
            {
                ethernet = Unsafe.As<byte, Ethernet>(ref MemoryMarshal.GetReference(span));
                // CRC check
                span = span.Slice(Unsafe.SizeOf<Ethernet>(), span.Length - (Unsafe.SizeOf<Ethernet>() + CrcSize));
                return true; 
            }
            
            ethernet = default;
            return false;
        }

        public override string ToString()
        {
            return "+- Ethernet Frame ---------------------------------------------------------------------+" + Environment.NewLine +
                  $"| {Ethertype.ToString().PadRight(11)} | {Source.ToString()} -> {Destination.ToString()}".PadRight(88) + "|";
        }
    }
}

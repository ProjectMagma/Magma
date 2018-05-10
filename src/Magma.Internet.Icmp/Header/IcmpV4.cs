using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Magma.Internet.Icmp;

namespace Magma.Network.Header
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct IcmpV4
    {
        private short _typeAndCode;

        public ControlMessage Type
        {
            get => (ControlMessage)(_typeAndCode & 0xFF);
            set => _typeAndCode = (short)((byte)value | (_typeAndCode & 0xFF00));
        }
        public Code Code
        {
            get => (Code)_typeAndCode;
            set => _typeAndCode = (short)value;
        }

        public ushort HeaderChecksum;
        public short Identifier;
        public short SequenceNumber;

        public static bool TryConsume(ref Span<byte> span, out IcmpV4 icmp)
        {
            if (span.Length >= Unsafe.SizeOf<IcmpV4>())
            {
                icmp = Unsafe.As<byte, IcmpV4>(ref MemoryMarshal.GetReference(span));
                // CRC check
                span = span.Slice(0, Unsafe.SizeOf<IcmpV4>());
                return true;
            }

            icmp = default;
            return false;
        }

        public unsafe override string ToString()
        {
            return "+- Icmp Datagram ----------------------------------------------------------------------+" + Environment.NewLine +
                  $"| {Type.ToString()} - {Code} | Id: {System.Net.IPAddress.NetworkToHostOrder(Identifier)} | Seq: {System.Net.IPAddress.NetworkToHostOrder(SequenceNumber)} ".PadRight(86) 
                  + "|";
        }

        public bool ValidateChecksum(int length)
        {
            var currentChecksum = HeaderChecksum;
            HeaderChecksum = 0;
            var newChecksum = Checksum.Calcuate(this, Unsafe.SizeOf<IcmpV4>());
            HeaderChecksum = currentChecksum;
            return currentChecksum == newChecksum ? true : false;
        }
    }
}

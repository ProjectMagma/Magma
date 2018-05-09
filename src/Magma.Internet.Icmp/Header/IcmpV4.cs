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
        public int Payload;

        public static bool TryConsume(ref Span<byte> span, out IcmpV4 icmp)
        {
            if (span.Length >= Unsafe.SizeOf<IcmpV4>())
            {
                icmp = Unsafe.As<byte, IcmpV4>(ref MemoryMarshal.GetReference(span));
                // CRC check
                return true;
            }

            icmp = default;
            return false;
        }

        public unsafe override string ToString()
        {
            return "+- Icmp Datagram ----------------------------------------------------------------------+" + Environment.NewLine +
                  $"| {Type.ToString()} - {Code} | Id: {System.Net.IPAddress.NetworkToHostOrder(Identifier)} | Seq: {System.Net.IPAddress.NetworkToHostOrder(SequenceNumber)} ".PadRight(86) +
                  (ValidateChecksum() ? " " : "X")
                  + "|"
                  + BitConverter.ToString(new Span<byte>(Unsafe.AsPointer(ref this), Unsafe.SizeOf<IcmpV4>()).ToArray());
        }

        public bool ValidateChecksum()
        {
            var currentChecksum = HeaderChecksum;
            HeaderChecksum = 0;
            var newChecksum = Checksum.Calcuate(this, Unsafe.SizeOf<IcmpV4>());
            HeaderChecksum = currentChecksum;
            return currentChecksum == newChecksum ? true : false;
        }
    }
}


using System.Runtime.InteropServices;

namespace Magma.Network
{
    public struct IPAddress
    {
        [StructLayout(LayoutKind.Sequential, Pack = 1, Size = 4)]
        public struct V4Address
        {
            public uint Address;

            public override string ToString() => $"{Address & 0xff}.{(Address >> 8) & 0xff}.{(Address >> 16) & 0xff}.{(Address >> 24) & 0xff}";

            public static bool operator ==(V4Address address1, V4Address address2) => address1.Address == address2.Address;
            public static bool operator !=(V4Address address1, V4Address address2) => address1.Address != address2.Address;

            public override int GetHashCode() => Address.GetHashCode();
            public override bool Equals(object obj) => obj is V4Address address2 && Address == address2.Address;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1, Size = 16)]
        public struct V6Address
        {
            uint _address0;
            uint _address1;
            uint _address2;
            uint _address3;
        }
    }
}

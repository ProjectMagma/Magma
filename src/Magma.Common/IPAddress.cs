
using System.Runtime.InteropServices;

namespace Magma.Network
{
    public struct IPAddress
    {
        [StructLayout(LayoutKind.Sequential, Pack = 1, Size = 4)]
        public struct V4Address
        {
            uint _address;
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

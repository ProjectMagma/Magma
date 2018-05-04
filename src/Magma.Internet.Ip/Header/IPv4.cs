
using System.Runtime.InteropServices;
using Magma.Internet.Ip;

using static Magma.Network.IPAddress;

namespace Magma.Network.Header
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct IPv4
    {
        // No Options
        private byte _versionAndHeaderLength;
        private byte _dscpAndEcn;
        private ushort _totalLength;
        private ushort _identification;
        private ushort _flagsAndFragmentOffset;
        private byte _ttl;
        private ProtocolNumber _protocol;
        private ushort _checksum;
        private V4Address _sourceIPAdress;
        private V4Address _destinationIPAdress;

        public byte Version => (byte)(_versionAndHeaderLength & 0b_0000_1111);
        public byte InternetHeaderLength => (byte)(_versionAndHeaderLength >> 4);
        public byte DifferentiatedServicesCodePoint => (byte)(_versionAndHeaderLength & 0x3f);
        public byte ExplicitCongestionNotification => (byte)(_versionAndHeaderLength >> 6);
        public ushort TotalLength => _totalLength;
        public ushort Identification => _identification;
        public byte Flags => (byte)(_flagsAndFragmentOffset >> 13);
        public ushort FragmentOffset => (ushort)(_flagsAndFragmentOffset & 0b_0001_1111_1111_1111);
        public byte TimeToLive => _ttl;
        public ProtocolNumber Protocol => _protocol;
        public ushort HeaderChecksum => _checksum;
        public V4Address SourceAddress => _sourceIPAdress;
        public V4Address DestinationAddress => _destinationIPAdress;
    }
}

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Magma.Network;
using Magma.Network.Header;
using Xunit;

namespace Magma.Common.Facts
{
    public class ChecksumFacts
    {
        [Theory]
        [InlineData(new byte[] {0x08, 0x00, 0x96, 0x5b, 0x00, 0x01, 0x00, 0xA3, 0x61})]
        [InlineData(new byte[] {0x08, 0x00, 0x95, 0xF8, 0x00, 0x01, 0x00, 0xA4, 0x61, 0x62})]
        [InlineData(new byte[] {0x08, 0x00, 0x96, 0x45, 0x00, 0x01, 0xb9, 0x61})]
        [InlineData(new byte[] {0x08, 0x00, 0x95, 0xE2, 0x00, 0x01, 0x00, 0xBa, 0x61, 0x62})]
        [InlineData(new byte[] {0x08, 0x00, 0x32, 0xE0, 0x00, 0x01, 0x00, 0xBC, 0x61, 0x62, 0x63})]
        [InlineData(new byte[] {0x08, 0x00, 0x32, 0x7B, 0x00, 0x01, 0x00, 0xBD, 0x61, 0x62, 0x63, 0x64})]
        public unsafe void IcpmChecksum(byte[] IcpmPacket)
        {
            var data = new Span<byte>(IcpmPacket);

            var parsed = data;
            Assert.True(IcmpV4.TryConsume(ref parsed, out var icmpIn));

            var checksum = icmpIn.HeaderChecksum;

            Assert.Equal(0, Checksum.Calcuate(MemoryMarshal.GetReference(data), IcpmPacket.Length));

            icmpIn.HeaderChecksum = 0;
            var changedData = new Span<byte>(&icmpIn, Unsafe.SizeOf<IcmpV4>());
            changedData.CopyTo(data);

            var newChecksum = Checksum.Calcuate(MemoryMarshal.GetReference(data), IcpmPacket.Length);

            Assert.Equal(checksum, newChecksum);
        }

    }
}

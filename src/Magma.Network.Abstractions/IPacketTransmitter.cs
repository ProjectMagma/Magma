using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Magma.Network.Abstractions
{
    public interface IPacketTransmitter
    {
        bool TryGetNextBuffer(out Memory<byte> buffer);
        Task SendBuffer(ReadOnlyMemory<byte> buffer);
        uint RandomSequenceNumber();
    }
}

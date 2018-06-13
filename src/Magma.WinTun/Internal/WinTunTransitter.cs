using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Magma.Network.Abstractions;

namespace Magma.WinTun.Internal
{
    public class WinTunTransitter : IPacketTransmitter
    {
        private readonly FileStream _fileStream;
        private readonly WinTunMemoryPool _pool;

        internal WinTunTransitter(FileStream fileStream, WinTunMemoryPool pool)
        {
            _fileStream = fileStream;
            _pool = pool;
        }

        public bool TryGetNextBuffer(out Memory<byte> buffer)
        {
            if (_pool.TryGetMemory(out var memManager))
            {
                buffer = memManager.Memory;
                return true;
            }
            buffer = default;
            return false;
        }

        public uint RandomSequenceNumber() => (uint)(new Random().Next());

        public async Task SendBuffer(ReadOnlyMemory<byte> buffer)
        {
            if (!MemoryMarshal.TryGetMemoryManager(buffer, out WinTunMemoryPool.WinTunOwnedMemory manager)) throw new InvalidOperationException();
            await _fileStream.WriteAsync(buffer);
            manager.Return();
        }
    }
}

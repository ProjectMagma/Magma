using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Runtime.CompilerServices;

namespace Magma.PCap
{
    public class PCapFileWriter:IDisposable
    {
        private FileStream _fileStream;
        private object _lock = new object();
        private int _maxSize;

        public unsafe PCapFileWriter(string fileName)
        {
            _fileStream = new FileStream(fileName, FileMode.Create);
            var header = FileHeader.Create();
            _maxSize = (int)header.SnapLength;
            var span = new Span<byte>(&header, Unsafe.SizeOf<FileHeader>());
            _fileStream.Write(span);
        }

        public void Dispose() => _fileStream.Dispose();

        public unsafe void WritePacket(Span<byte> packet)
        {
            var time = (DateTime.UtcNow - new DateTime(1970, 1, 1));
            var header = new RecordHeader()
            {
                incl_len = packet.Length,
                orig_len = packet.Length,
                ts_sec = (uint)time.TotalSeconds,
                ts_usec = (uint)time.Milliseconds,
            };
            var headerSpan = new Span<byte>(&header, Unsafe.SizeOf<RecordHeader>());
            lock(_lock)
            {
                _fileStream.Write(headerSpan);
                _fileStream.Write(packet);
            }
        }
    }
}

using System;
using System.Linq;
using BenchmarkDotNet.Attributes;
using Magma.Network;

namespace Magma.Performance
{
    public class ChecksumBenchmark
    {
        private byte[] _bytes;

        [Params(16, 64, 256, 1024)]
        public int Bytes { get; set; }

        [Benchmark]
        public ushort GenerateChecksum() => Checksum.Calculate(ref _bytes[0], _bytes.Length);

        [Benchmark]
        public bool ValidateChecksum() => Checksum.IsValid(ref _bytes[0], _bytes.Length);

        [GlobalSetup]
        public void GlobalSetup()
        {
            _bytes = Enumerable.Range(0, Bytes).Select(x => (byte)x).ToArray();
        }
    }
}

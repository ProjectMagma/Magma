using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace Magma.Transport.Tcp.Header
{
    public struct TcpHeaderWithOptions
    {
        public Network.Header.Tcp Header;
        private byte _windowScale;
        private ushort _maximumSegmentSize;
        private bool _sackPermitted;
        private uint _timeStamp;
        private uint _timeStampEchoReply;

        public bool SackPermitted => _sackPermitted;
        public ushort MaximumSegmentSize => _maximumSegmentSize;
        public byte WindowScale => _windowScale;
        public uint TimeStamp => _timeStamp;

        public static bool TryConsume(ReadOnlySpan<byte> input, out TcpHeaderWithOptions headerWithOps, out ReadOnlySpan<byte> data)
        {
            if (!Network.Header.Tcp.TryConsume(input, out var tcpHeader, out var options, out data))
            {
                headerWithOps = default;
                return false;
            }

            headerWithOps = new TcpHeaderWithOptions() { Header = tcpHeader };

            var exit = false;
            var originalOptions = options;
            try
            {
                while (options.Length > 0 && !exit)
                {
                    var optionKind = (TcpOptionKind)options[0];
                    switch (optionKind)
                    {
                        case TcpOptionKind.WindowScale:
                            headerWithOps._windowScale = options[2];
                            options = options.Slice(3);
                            break;
                        case TcpOptionKind.MaximumSegmentSize:
                            headerWithOps._maximumSegmentSize = (ushort)(options[2] << 8 | options[3]);
                            options = options.Slice(4);
                            break;
                        case TcpOptionKind.NoOp:
                            options = options.Slice(1);
                            break;
                        case TcpOptionKind.SackPermitted:
                            headerWithOps._sackPermitted = true;
                            options = options.Slice(2);
                            break;
                        case TcpOptionKind.Timestamps:
                            headerWithOps._timeStamp = System.Buffers.Binary.BinaryPrimitives.ReadUInt32BigEndian(options.Slice(2));
                            headerWithOps._timeStampEchoReply = System.Buffers.Binary.BinaryPrimitives.ReadUInt32BigEndian(options.Slice(6));
                            options = options.Slice(10);
                            break;
                        case TcpOptionKind.EndOfOptions:
                            exit = true;
                            break;
                        default:
                            Console.WriteLine($"Unknown option kind {optionKind}");
                            options = options.Slice(options[1]);
                            break;
                    }
                }
            }
            catch
            {
                Console.WriteLine($"Failed to parse options data was {BitConverter.ToString(originalOptions.ToArray())}");
            }
            return true;
        }

        public static int SizeOfSynAckHeader = Unsafe.SizeOf<Network.Header.Tcp>() + Unsafe.SizeOf<TcpOptionMaxSegmentSize>() + Unsafe.SizeOf<TcpOptionTimestamp>() + Unsafe.SizeOf<TcpOptionWindowScale>();
    }
}

using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Magma.Link;
using Magma.Network;
using Magma.Network.Header;
using Magma.Transport.Tcp.Header;
using Microsoft.AspNetCore.Server.Kestrel.Transport.Abstractions.Internal;
using static Magma.Network.IPAddress;

namespace Magma.Transport.Tcp
{
    public abstract class TcpConnection : TransportConnection
    {
        private TcpConnectionState _state = TcpConnectionState.Listen;
        private uint _receiveSequenceNumber;
        private uint _sendAckSequenceNumber;
        private uint _sendSequenceNumber;
        private ushort _remotePort;
        private ushort _localPort;
        private V4Address _remoteAddress;
        private V4Address _localAddress;
        private Ethernet _outboundEthernetHeader;
        private Network.Header.Tcp _partialTcpHeader;
        private IPv4 _partialIPHeader;
        private byte _windowScale;
        private ushort _windowSize;
        private ulong _pseudoPartialSum;
        private uint _echoTimestamp;
        private Task _flushTask = Task.CompletedTask;
        private IConnectionDispatcher _connectionDispatcher;
        private CancellationTokenSource _closedToken = new CancellationTokenSource();
        private long _totalBytesWritten;

        public TcpConnection(Ethernet ethHeader, 
            IPv4 ipHeader, 
            Network.Header.Tcp tcpHeader, 
            PipeScheduler readScheduler, 
            PipeScheduler writeScheduler, 
            MemoryPool<byte> memoryPool, 
            IConnectionDispatcher connectionDispatcher)
        {
            _connectionDispatcher = connectionDispatcher;
            _remoteAddress = ipHeader.SourceAddress;
            _localAddress = ipHeader.DestinationAddress;
            _outboundEthernetHeader = new Ethernet() { Destination = ethHeader.Source, Ethertype = EtherType.IPv4, Source = ethHeader.Destination };
            var pseudo = new TcpV4PseudoHeader() { Destination = _remoteAddress, Source = _localAddress, ProtocolNumber = Internet.Ip.ProtocolNumber.Tcp, Reserved = 0 };
            _pseudoPartialSum = Checksum.PartialCalculate(ref Unsafe.As<TcpV4PseudoHeader, byte>(ref pseudo), Unsafe.SizeOf<TcpV4PseudoHeader>());

            LocalAddress = new System.Net.IPAddress(_localAddress.Address);
            RemoteAddress = new System.Net.IPAddress(_remoteAddress.Address);
            RemotePort = tcpHeader.SourcePort;
            LocalPort = tcpHeader.DestinationPort;

            OutputReaderScheduler = readScheduler ?? throw new ArgumentNullException(nameof(readScheduler));
            InputWriterScheduler = writeScheduler ?? throw new ArgumentNullException(nameof(writeScheduler));
            MemoryPool = memoryPool ?? throw new ArgumentNullException(nameof(memoryPool));

            ConnectionClosed = _closedToken.Token;
        }

        public override PipeScheduler OutputReaderScheduler { get; }
        public override PipeScheduler InputWriterScheduler { get; }
        public override MemoryPool<byte> MemoryPool { get; }
        public override long TotalBytesWritten => _totalBytesWritten;
        public bool PendingAck { get; set; }
        
        public void ProcessPacket(TcpHeaderWithOptions header, ReadOnlySpan<byte> data)
        {
            // If there is backpressure just drop the packet
            if (!_flushTask.IsCompleted) return;

            _echoTimestamp = header.TimeStamp;
            switch (_state)
            {
                case TcpConnectionState.Listen:
                    _sendSequenceNumber = GetRandomSequenceStart();
                    _receiveSequenceNumber = header.Header.SequenceNumber;
                    // We know we checked for syn in the upper layer so we can ignore that for now
                    _partialTcpHeader = Network.Header.Tcp.Create(_localPort, _remotePort);
                    WriteSyncAckPacket();
                    _state = TcpConnectionState.Syn_Rcvd;
                    break;
                case TcpConnectionState.Syn_Rcvd:
                    if (header.Header.SYN)
                    {
                        Console.WriteLine("Another Syn made");
                        return;
                    }
                    _connectionDispatcher.OnConnection(this);
                    _state = TcpConnectionState.Established;
                    break;
                case TcpConnectionState.Established:
                    if(header.Header.FIN) { throw new NotImplementedException("Got a fin don't know what to do, dying"); }
                    if(header.Header.RST) { throw new NotImplementedException("Got an rst don't know what to do dying"); }

                    // First lets update our acked squence number;
                    _sendAckSequenceNumber = header.Header.AcknowledgmentNumber;

                    if(_receiveSequenceNumber != header.Header.SequenceNumber)
                    {
                        // We are just going to drop this and wait for a resend
                        Console.WriteLine("Dropped packet due to wrong sequence");
                        Console.WriteLine($"Expected seq {_receiveSequenceNumber} got {header.Header.SequenceNumber}");
                    }
                    unchecked { _receiveSequenceNumber += (uint)data.Length; }
                    var output = Input.GetMemory(data.Length);
                    data.CopyTo(output.Span);
                    Input.Advance(data.Length);
                    // need to do something with the task and make sure we don't overlap the writes
                    var task = Input.FlushAsync();
                    if (!task.IsCompleted)
                    {
                        _flushTask = task.AsTask();
                    }
                    PendingAck = true;
                    _totalBytesWritten += data.Length;
                    Console.WriteLine("Posted data to connection");
                    WriteDataPacket();
                    break;
                default:
                    throw new NotImplementedException($"Unknown tcp state?? {_state}");
            }
        }

        public void SendAckIfRequired()
        {
            if(PendingAck)
            {
                PendingAck = false;
                WriteAckPacket();
            }
        }

        private void WriteAckPacket()
        {
            if (!TryGetMemory(out var memory)) throw new InvalidOperationException("Back pressure, something to do here");
            var totalSize = Unsafe.SizeOf<Ethernet>() + Unsafe.SizeOf<IPv4>() + TcpHeaderWithOptions.SizeOfStandardHeader;
            memory = memory.Slice(0, totalSize);
            var span = memory.Span.Slice(0, totalSize);
            ref var pointer = ref MemoryMarshal.GetReference(span);

            Unsafe.WriteUnaligned(ref pointer, _outboundEthernetHeader);
            pointer = ref Unsafe.Add(ref pointer, Unsafe.SizeOf<Ethernet>());

            ref var ipHeader = ref Unsafe.As<byte, IPv4>(ref pointer);
            IPv4.InitHeader(ref ipHeader, _localAddress, _remoteAddress, (ushort)TcpHeaderWithOptions.SizeOfStandardHeader, Internet.Ip.ProtocolNumber.Tcp, 0);
            pointer = ref Unsafe.Add(ref pointer, Unsafe.SizeOf<IPv4>());

            ref var header = ref _partialTcpHeader;
            header.AcknowledgmentNumber = _receiveSequenceNumber;
            header.SequenceNumber = _sendSequenceNumber;
            header.WindowSize = 50;
            Unsafe.WriteUnaligned(ref pointer, header);
            header = ref Unsafe.As<byte, Network.Header.Tcp>(ref pointer);
            header.SetChecksum(span.Slice(span.Length - TcpHeaderWithOptions.SizeOfStandardHeader), _pseudoPartialSum);

            WriteMemory(memory);
            PendingAck = false;
        }

        private void WriteDataPacket()
        {
            var content = "<html><body><h1>Hello from Magma</h1></body></html>";

            var fullResponse = "HTTP/1.1 200 OK\r\n" +
            "Date: Tue, 29 Mar 2018 23:19:53 GMT\r\n" +
            "Server: Magma\r\n" +
            "Content-Length: " + content.Length + "\r\n" +
            "Content-Type: text/plain\r\n" + "\r\n" + content;

            var bytes = Encoding.UTF8.GetBytes(fullResponse);
            WriteDataPacket(bytes);
        }

        private void WriteDataPacket(Span<byte> data)
        {
            if (!TryGetMemory(out var memory)) throw new InvalidOperationException("Back pressure, something to do here");
            var totalSize = Unsafe.SizeOf<Ethernet>() + Unsafe.SizeOf<IPv4>() + TcpHeaderWithOptions.SizeOfStandardHeader + data.Length;
            memory = memory.Slice(0, totalSize);
            var span = memory.Span;
            ref var pointer = ref MemoryMarshal.GetReference(span);

            Unsafe.WriteUnaligned(ref pointer, _outboundEthernetHeader);
            pointer = ref Unsafe.Add(ref pointer, Unsafe.SizeOf<Ethernet>());

            ref var ipHeader = ref Unsafe.As<byte, IPv4>(ref pointer);
            IPv4.InitHeader(ref ipHeader, _localAddress, _remoteAddress, (ushort)(TcpHeaderWithOptions.SizeOfStandardHeader + data.Length), Internet.Ip.ProtocolNumber.Tcp, 0);
            pointer = ref Unsafe.Add(ref pointer, Unsafe.SizeOf<IPv4>());

            ref var tcpHeader = ref Unsafe.As<byte, Network.Header.Tcp>(ref pointer);
            tcpHeader.DestinationPort = _remotePort;
            tcpHeader.SourcePort = _localPort;
            tcpHeader.AcknowledgmentNumber = _receiveSequenceNumber;
            tcpHeader.SequenceNumber = _sendSequenceNumber;
            tcpHeader.ACK = true;
            tcpHeader.Checksum = 0;
            tcpHeader.CWR = false;
            tcpHeader.DataOffset = (byte)(TcpHeaderWithOptions.SizeOfStandardHeader / 4);
            tcpHeader.ECE = false;
            tcpHeader.FIN = false;
            tcpHeader.NS = false;
            tcpHeader.PSH = true;
            tcpHeader.RST = false;
            tcpHeader.SYN = false;
            tcpHeader.URG = false;
            tcpHeader.UrgentPointer = 0;
            tcpHeader.WindowSize = 50;

            data.CopyTo(span.Slice(span.Length - data.Length));

            tcpHeader.SetChecksum(span.Slice(span.Length - (TcpHeaderWithOptions.SizeOfStandardHeader + data.Length)), _pseudoPartialSum);
            unchecked { _sendSequenceNumber += (uint)data.Length; }

            WriteMemory(memory);
            PendingAck = false;
        }

        private void WriteSyncAckPacket()
        {
            if (!TryGetMemory(out var memory)) throw new InvalidOperationException("Back pressure, something to do here");
            var totalSize = Unsafe.SizeOf<Ethernet>() + Unsafe.SizeOf<IPv4>() + TcpHeaderWithOptions.SizeOfSynAckHeader;
            memory = memory.Slice(0, totalSize);
            var span = memory.Span;
            ref var pointer = ref MemoryMarshal.GetReference(span);

            Unsafe.WriteUnaligned(ref pointer, _outboundEthernetHeader);
            pointer = ref Unsafe.Add(ref pointer, Unsafe.SizeOf<Ethernet>());

            ref var ipHeader = ref Unsafe.As<byte, IPv4>(ref pointer);
            IPv4.InitHeader(ref ipHeader, _localAddress, _remoteAddress, (ushort)TcpHeaderWithOptions.SizeOfSynAckHeader, Internet.Ip.ProtocolNumber.Tcp, 0);
            pointer = ref Unsafe.Add(ref pointer, Unsafe.SizeOf<IPv4>());

            ref var tcpHeader = ref Unsafe.As<byte, Network.Header.Tcp>(ref pointer);
            tcpHeader.DestinationPort = _remotePort;
            tcpHeader.SourcePort = _localPort;
            tcpHeader.AcknowledgmentNumber = ++_receiveSequenceNumber;
            tcpHeader.SequenceNumber = _sendSequenceNumber++;
            tcpHeader.ACK = true;
            tcpHeader.Checksum = 0;
            tcpHeader.CWR = false;
            tcpHeader.DataOffset = (byte)(TcpHeaderWithOptions.SizeOfSynAckHeader / 4);
            tcpHeader.ECE = false;
            tcpHeader.FIN = false;
            tcpHeader.NS = false;
            tcpHeader.PSH = false;
            tcpHeader.RST = false;
            tcpHeader.SYN = true;
            tcpHeader.URG = false;
            tcpHeader.UrgentPointer = 0;
            tcpHeader.WindowSize = 5792;
            ref var optionPoint = ref Unsafe.Add(ref pointer, Unsafe.SizeOf<Network.Header.Tcp>());

            var maxSegmentSize = new TcpOptionMaxSegmentSize(1440);
            Unsafe.WriteUnaligned(ref optionPoint, maxSegmentSize);
            optionPoint = ref Unsafe.Add(ref optionPoint, Unsafe.SizeOf<TcpOptionMaxSegmentSize>());

            var timestamps = new TcpOptionTimestamp(GetTimestamp(), _echoTimestamp);
            Unsafe.WriteUnaligned(ref optionPoint, timestamps);
            optionPoint = ref Unsafe.Add(ref optionPoint, Unsafe.SizeOf<TcpOptionTimestamp>());

            var windowScale =new TcpOptionWindowScale(9);
            Unsafe.WriteUnaligned(ref optionPoint, windowScale);

            tcpHeader.SetChecksum(span.Slice(span.Length - TcpHeaderWithOptions.SizeOfSynAckHeader), _pseudoPartialSum);

            WriteMemory(memory);
        }
                
        protected abstract void WriteMemory(Memory<byte> memory);
        protected abstract bool TryGetMemory(out Memory<byte> memory);
        protected abstract uint GetRandomSequenceStart();
        protected abstract uint GetTimestamp();
    }
}

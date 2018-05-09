using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using Magma.NetMap.Interop;
using Magma.Network.Abstractions;

namespace Magma.NetMap
{
    public class NetMapPort<TPacketReceiver>
        where TPacketReceiver : IPacketReceiver
    {
        private NetMapReceiveRing<TPacketReceiver>[] _receiveRings;
        private NetMapTransmitRing[] _transmitRings;
        private List<NetMapRing> _allRings;
        private NetMapHostRxRing _hostRxRing;
        private NetMapTransmitRing _hostTxRing;
        private readonly string _interfaceName;
        private NetMapRequest _request;
        private int _fileDescriptor;
        private IntPtr _mappedRegion;
        private NetMapInterface _netmapInterface;
        private Func<NetMapTransmitRing, TPacketReceiver> _createReceiver;

        public NetMapPort(string interfaceName, Func<NetMapTransmitRing, TPacketReceiver> createReceiver)
        {
            _interfaceName = interfaceName;
            _createReceiver = createReceiver;
        }

        public NetMapTransmitRing[] TransmitRings => _transmitRings;

        private IntPtr NetMapInterfaceAddress => IntPtr.Add(_mappedRegion, (int)_request.nr_offset);

        public unsafe void Open()
        {
            var request = new NetMapRequest
            {
                nr_cmd = 0,
                nr_flags = 0x0003,
                nr_ringid = 0,
                nr_version = Consts.NETMAP_API,
                
            };
            var textbytes = Encoding.ASCII.GetBytes(_interfaceName + "\0");
            fixed (void* txtPtr = textbytes)
            {
                Buffer.MemoryCopy(txtPtr, request.nr_name, textbytes.Length, textbytes.Length);
            }
            _fileDescriptor = Unix.Open("/dev/netmap", Unix.OpenFlags.O_RDWR);
            if (_fileDescriptor < 0) throw new InvalidOperationException("Need to handle properly (release memory etc)");
            if (Unix.IOCtl(_fileDescriptor, Consts.NIOCREGIF, &request) != 0)
            {
                throw new InvalidOperationException("Some failure to get the port, need better error handling");
            }
            _request = request;
            
            MapMemory();
            SetupRings();

            var maxBufferId = _allRings.Select(r => r.GetMaxBufferId()).Max();
            var buffersStart = _allRings[0].BufferStart;
            var pool = new NetMapBufferPool((ushort)_allRings[0].BufferSize, buffersStart, maxBufferId + 1);
            foreach(var ring in _allRings)
            {
                ring.BufferPool = pool;
            }
        }

        private unsafe void SetupRings()
        {
            _allRings = new List<NetMapRing>();

            var txOffsets = new ulong[_netmapInterface.NumberOfTXRings];
            var rxOffsets = new ulong[_netmapInterface.NumberOfRXRings];
            var span = new Span<byte>(IntPtr.Add(NetMapInterfaceAddress, Unsafe.SizeOf<NetMapInterface>()).ToPointer(), (int)((_netmapInterface.NumberOfRXRings + _netmapInterface.NumberOfTXRings + 2) * sizeof(IntPtr)));
            for (var i = 0; i < txOffsets.Length; i++)
            {
                txOffsets[i] = System.Buffers.Binary.BinaryPrimitives.ReadUInt64LittleEndian(span);
                span = span.Slice(sizeof(ulong));
            }
            var txHost = System.Buffers.Binary.BinaryPrimitives.ReadUInt64LittleEndian(span);
            span = span.Slice(sizeof(ulong));
            
            for (var i = 0; i < rxOffsets.Length; i++)
            {
                rxOffsets[i] = System.Buffers.Binary.BinaryPrimitives.ReadUInt64LittleEndian(span);
                span = span.Slice(sizeof(ulong));
            }
            var rxHost = System.Buffers.Binary.BinaryPrimitives.ReadUInt64LittleEndian(span);

            _hostRxRing = new NetMapHostRxRing((byte*)_mappedRegion.ToPointer(), rxHost, _fileDescriptor, _transmitRings[0]);
            _hostTxRing = new NetMapTransmitRing((byte*)_mappedRegion.ToPointer(), txHost, _fileDescriptor);
            _allRings.Add(_hostRxRing);
            _allRings.Add(_hostTxRing);

            _transmitRings = new NetMapTransmitRing[txOffsets.Length];
            for (var i = 0; i < txOffsets.Length; i++)
            {
                _transmitRings[i] = new NetMapTransmitRing((byte*)_mappedRegion.ToPointer(), txOffsets[i], _fileDescriptor);
                _allRings.Add(_transmitRings[i]);
            }

            _receiveRings = new NetMapReceiveRing<TPacketReceiver>[rxOffsets.Length];
            for(var i = 0; i < rxOffsets.Length;i++)
            {
                _receiveRings[i] = new NetMapReceiveRing<TPacketReceiver>((byte*)_mappedRegion.ToPointer(), rxOffsets[i], _fileDescriptor, _createReceiver(_transmitRings[i]), _hostTxRing);
                _allRings.Add(_transmitRings[i]);
            }
        }

        private unsafe void MapMemory()
        {
            var mapResult = Unix.MMap(IntPtr.Zero, _request.nr_memsize, Unix.MemoryMappedProtections.PROT_READ | Unix.MemoryMappedProtections.PROT_WRITE, Unix.MemoryMappedFlags.MAP_SHARED, _fileDescriptor, offset: 0);

            if ((long)mapResult < 0) throw new InvalidOperationException("Failed to map the memory");

            Console.WriteLine("Mapped the memory region correctly");
            _mappedRegion = mapResult;
            _netmapInterface = Unsafe.Read<NetMapInterface>(NetMapInterfaceAddress.ToPointer());
        }
        
        public void PrintPortInfo()
        {
            Console.WriteLine($"memsize = {_request.nr_memsize}");
            Console.WriteLine($"tx rings = {_request.nr_tx_rings}");
            Console.WriteLine($"rx rings = {_request.nr_rx_rings}");
            Console.WriteLine($"tx slots = {_request.nr_tx_slots}");
            Console.WriteLine($"rx slots = {_request.nr_rx_slots}");
            Console.WriteLine($"Offset to IF Header {_request.nr_offset}");
            Console.WriteLine($"Interface Nic RX Queues {_netmapInterface.NumberOfRXRings}");
            Console.WriteLine($"Interface Nic TX Queues {_netmapInterface.NumberOfTXRings}");
            Console.WriteLine($"Interface Start of extra buffers {_netmapInterface.ni_bufs_head}");
        }
    }
}

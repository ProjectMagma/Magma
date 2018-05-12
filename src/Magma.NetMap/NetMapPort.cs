using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using Magma.NetMap.Interop;
using Magma.Network.Abstractions;
using static Magma.NetMap.Interop.Libc;
using static Magma.NetMap.Interop.Netmap;

namespace Magma.NetMap
{
    public class NetMapPort<TPacketReceiver>
        where TPacketReceiver : IPacketReceiver
    {
        private NetMapReceiveRing<TPacketReceiver>[] _receiveRings;
        private NetMapTransmitRing[] _transmitRings;
        private List<NetMapRing> _allRings = new List<NetMapRing>();
        private NetMapHostRxRing _hostRxRing;
        private NetMapTransmitRing _hostTxRing;
        private readonly string _interfaceName;
        private NetMapRequest _request;
        private FileDescriptor _fileDescriptor;
        private IntPtr _mappedRegion;
        private NetMapInterface _netmapInterface;
        private readonly Func<NetMapTransmitRing, TPacketReceiver> _createReceiver;

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
                nr_flags = NetMapRequestFlags.NR_REG_NIC_SW,
                nr_ringid = 0,
                nr_version = NETMAP_API,
                
            };
            var textbytes = Encoding.ASCII.GetBytes(_interfaceName + "\0");
            fixed (void* txtPtr = textbytes)
            {
                Buffer.MemoryCopy(txtPtr, request.nr_name, textbytes.Length, textbytes.Length);
            }
            _fileDescriptor = Libc.Open("/dev/netmap", OpenFlags.O_RDWR);
            if (!_fileDescriptor.IsValid) throw new InvalidOperationException("Unable to open /dev/netmap is the kernel module running? Have you run with sudo?");
            if (IOCtl(_fileDescriptor, IOControlCommand.NIOCREGIF, ref request) != 0)
            {
                throw new InvalidOperationException($"Netmap opened but unable to open the interface {_interfaceName}");
            }
            _request = request;
            
            MapMemory();
            SetupRings();

            var maxBufferId = _allRings.Select(r => r.GetMaxBufferId()).Max();
            var buffersStart = _allRings[0].BufferStart;
            if (_allRings[0].BufferStart != _allRings[_allRings.Count - 1].BufferStart) throw new InvalidOperationException("Buffer start doesn't match!!");
            var pool = new NetMapBufferPool((ushort)_allRings[0].BufferSize, buffersStart, maxBufferId + 1);
            foreach(var ring in _allRings)
            {
                ring.BufferPool = pool;
                if(ring is NetMapReceiveRing<TPacketReceiver> receiver)
                {
                    receiver.Start();
                }
                if(ring is NetMapHostRxRing hostRing)
                {
                    hostRing.Start();
                }
            }
        }

        private unsafe void SetupRings()
        {
            var txOffsets = new long[_netmapInterface.NumberOfTXRings];
            var rxOffsets = new long[_netmapInterface.NumberOfRXRings];
            var rxTxPairs = new RxTxPair[txOffsets.Length];
            var span = new Span<long>(IntPtr.Add(NetMapInterfaceAddress, Unsafe.SizeOf<NetMapInterface>()).ToPointer(), _netmapInterface.NumberOfRXRings + _netmapInterface.NumberOfTXRings + 2);
            for (var i = 0; i < txOffsets.Length; i++)
            {
                txOffsets[i] = span[0];
                span = span.Slice(1);
                rxTxPairs[i] = new RxTxPair(_interfaceName, i, false);
            }
            var rxTxPairHost = new RxTxPair(_interfaceName, txOffsets.Length, true);

            var txHost = span[0];
            span = span.Slice(1);
            
            for (var i = 0; i < rxOffsets.Length; i++)
            {
                rxOffsets[i] = span[0];
                span = span.Slice(1);
            }
            var rxHost = span[0];

            _hostTxRing = new NetMapTransmitRing(rxTxPairHost, (byte*)_mappedRegion.ToPointer(), txHost);
            _allRings.Add(_hostTxRing);
            _transmitRings = new NetMapTransmitRing[txOffsets.Length];
            _receiveRings = new NetMapReceiveRing<TPacketReceiver>[rxOffsets.Length];
            for (var i = 0; i < txOffsets.Length; i++)
            {
                _transmitRings[i] = new NetMapTransmitRing(rxTxPairs[i], (byte*)_mappedRegion.ToPointer(), txOffsets[i]);
                _allRings.Add(_transmitRings[i]);
                _receiveRings[i] = new NetMapReceiveRing<TPacketReceiver>(rxTxPairs[i], (byte*)_mappedRegion.ToPointer(), rxOffsets[i], _createReceiver(_transmitRings[i]), _hostTxRing);
                _allRings.Add(_receiveRings[i]);
            }
            _hostRxRing = new NetMapHostRxRing(rxTxPairHost, (byte*)_mappedRegion.ToPointer(), rxHost, _transmitRings[0]);
            _allRings.Add(_hostRxRing);
        }

        private unsafe void MapMemory()
        {
            var mapResult = MMap(IntPtr.Zero, _request.nr_memsize, MemoryMappedProtections.PROT_READ | MemoryMappedProtections.PROT_WRITE, MemoryMappedFlags.MAP_SHARED, _fileDescriptor, offset: 0);

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

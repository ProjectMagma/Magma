using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using Magma.NetMap.Interop;
using Magma.Network.Header;
using static Magma.NetMap.Interop.Libc;

namespace Magma.NetMap.Internal
{
    internal sealed class NetMapHostRxRing : INetMapRing
    {
        private readonly Thread _worker;
        private readonly NetMapTransmitRing _transmitRing;
        private readonly NetMapRing _netMapRing;
        private NetMapBufferPool _bufferPool;

        internal unsafe NetMapHostRxRing(string interfaceName, byte* memoryRegion, long queueOffset, NetMapTransmitRing transmitRing)
        {
            _transmitRing = transmitRing;
            _netMapRing = new NetMapRing(interfaceName, isHostStack: true, isTxRing: false, memoryRegion, queueOffset);
            _worker = new Thread(new ThreadStart(ThreadLoop));
        }

        public NetMapRing NetMapRing => _netMapRing;
        public NetMapBufferPool BufferPool { set => _bufferPool = value; }

        public void Start() => _worker.Start();

        public void Return(int buffer_index) => throw new NotImplementedException();

        private void ThreadLoop()
        {
            ref var ring = ref _netMapRing.RingInfo;
            while (true)
            {
                while (!_netMapRing.IsRingEmpty())
                {
                    //Console.WriteLine("Received data on host ring");
                    var i = ring.Cursor;
                    ring.Cursor = _netMapRing.RingNext(i);
                    if (_transmitRing.TryGetNextBuffer(out var copyBuffer))
                    {
                        ref var slot = ref _netMapRing.GetSlot(i);
                        var bufferSource = _netMapRing.GetBuffer(slot.buf_idx);
                        bufferSource.CopyTo(copyBuffer.Span);
                        _transmitRing.SendBuffer(copyBuffer.Slice(0, slot.len));
                    }
                    ring.Head = _netMapRing.RingNext(ring.Head);
                }
                _netMapRing.WaitForWork();
            }
        }

        public void Dispose() => _netMapRing.Dispose();
    }
}

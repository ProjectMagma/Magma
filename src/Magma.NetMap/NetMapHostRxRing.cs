using System.Threading;
using Magma.NetMap.Interop;
using static Magma.NetMap.Interop.Libc;

namespace Magma.NetMap
{
    public sealed class NetMapHostRxRing:NetMapRing
    {
        private readonly Thread _worker;
        private readonly NetMapTransmitRing _transmitRing;

        internal unsafe NetMapHostRxRing(string interfaceName, byte* memoryRegion, ulong rxQueueOffset, FileDescriptor fileDescriptor, NetMapTransmitRing transmitRing)
            : base(interfaceName, isTxRing : false, isHost:true, memoryRegion, rxQueueOffset)
        {
            _transmitRing = transmitRing;
            _worker = new Thread(new ThreadStart(ThreadLoop));
            _worker.IsBackground = true;
            _worker.Start();
        }

        private void ThreadLoop()
        {
            ref var ring = ref RingInfo();
            while (true)
            {
                var fd = new PollFileDescriptor()
                {
                    Events = PollEvents.POLLIN,
                    Fd = _fileDescriptor
                };

                var pollResult = Poll(ref fd, 1, -1);
                if (pollResult < 0)
                {
                    return;
                }
                var sentData = false;
                while (!IsRingEmpty())
                {
                    var i = ring.Cursor;
                    
                    _transmitRing.TrySendWithSwap(ref GetSlot(i), ref ring);
                    sentData = true;
                }
                if(sentData) _transmitRing.ForceFlush();
            }
        }
    }
}

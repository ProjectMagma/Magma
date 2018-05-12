using System;
using System.Text;
using static Magma.NetMap.Interop.Libc;
using static Magma.NetMap.Interop.Netmap;

namespace Magma.NetMap.Interop
{
    public class RxTxPair
    {
        private FileDescriptor _fileDescriptor;
        private FileDescriptor _eventDescriptor;
        private int _ringId;
        private bool _isHostStack;

        internal unsafe RxTxPair(string interfaceName, int ringId, bool isHostStack)
        {
            _ringId = ringId;
            _isHostStack = isHostStack;
            _fileDescriptor = Open("/dev/netmap", OpenFlags.O_RDWR);
            if (!_fileDescriptor.IsValid) ExceptionHelper.ThrowInvalidOperation($"Unable to open the /dev/netmap device {_fileDescriptor}");
            var request = new NetMapRequest
            {
                nr_cmd = 0,
                nr_ringid = (ushort)_ringId,
                nr_version = NETMAP_API,
            };
            if (_isHostStack)
            {
                request.nr_flags = NetMapRequestFlags.NR_REG_SW;
                request.nr_ringid = 0;
            }
            else
            {
                request.nr_flags = NetMapRequestFlags.NR_REG_ONE_NIC;
            }
            var textbytes = Encoding.ASCII.GetBytes(interfaceName + "\0");
            fixed (void* txtPtr = textbytes)
            {
                Buffer.MemoryCopy(txtPtr, request.nr_name, textbytes.Length, textbytes.Length);
            }
            if (IOCtl(_fileDescriptor, IOControlCommand.NIOCREGIF, ref request) != 0) ExceptionHelper.ThrowInvalidOperation("Failed to open an FD for a single ring");

            _eventDescriptor = CreateEventFD(0, EventFDFlags.EFD_SEMAPHORE | EventFDFlags.EFD_NONBLOCK);
        }

        public unsafe void WaitForWork()
        {
            var pfd = new PollFileDescriptor()
            {
                Events = PollEvents.POLLIN,
                Fd = _fileDescriptor,
            };
            var result = Poll(ref pfd, 1, 1000);
            if (result < 0) ExceptionHelper.ThrowInvalidOperation("Error on poll");

        }

        public unsafe void ForceFlush() => IOCtl(_fileDescriptor, IOControlCommand.NIOCTXSYNC, IntPtr.Zero);
    }
}

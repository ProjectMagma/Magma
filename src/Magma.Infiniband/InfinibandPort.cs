using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using static Magma.Infiniband.Interop.IbvContext;
using static Magma.Interop.Linux.Libc;

namespace Magma.Infiniband
{
    public class InfinibandPort
    {
        private IntPtr _mappedRegion;
        private int _buffers;
        private int _bufferSize;

        public unsafe InfinibandPort(int buffers, int bufferSize)
        {
            Console.WriteLine($"Size of context_ops {Unsafe.SizeOf<ibv_context_ops>()}");
            Console.WriteLine($"Size of context {Unsafe.SizeOf<ibv_context>()}");
            _buffers = buffers;
            _bufferSize = bufferSize;
            Console.WriteLine("Getting device list");
            ref var devices = ref Interop.IbvDevice.ibv_get_device_list(out var numberOfDevices);
            Console.WriteLine($"Number of devices found {numberOfDevices}");
            //_mappedRegion = MMap(IntPtr.Zero, (ulong)_buffers * (ulong)_bufferSize, MemoryMappedProtections.PROT_READ | MemoryMappedProtections.PROT_WRITE, MemoryMappedFlags.MAP_PRIVATE | MemoryMappedFlags.MAP_ANONYMOUS, new FileDescriptor(), 0);
        }


    }
}

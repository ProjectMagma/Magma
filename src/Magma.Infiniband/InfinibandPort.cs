using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using Magma.Infiniband.Interop;
using static Magma.Infiniband.Interop.IbvContext;
using static Magma.Infiniband.Interop.IbvDevice;
using static Magma.Interop.Linux.Libc;

namespace Magma.Infiniband
{
    public unsafe class InfinibandPort
    {
        private IntPtr _mappedRegion;
        private int _buffers;
        private int _bufferSize;
        private ibv_context* _context;
        private query_device _queryDevice;

        [SuppressUnmanagedCodeSecurity]
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int query_device(ibv_context* context,out ibv_device_attr attributes);

        public InfinibandPort(int buffers, int bufferSize, string deviceName)
        {
            Console.WriteLine($"Size of context_ops {Unsafe.SizeOf<ibv_context_ops>()}");
            Console.WriteLine($"Size of context {Unsafe.SizeOf<ibv_context>()}");
            Console.WriteLine($"Size of attr {Unsafe.SizeOf<ibv_device_attr>()}");
            _buffers = buffers;
            _bufferSize = bufferSize;
            Console.WriteLine("Getting device list");
            _context = OpenContext(deviceName);
            _queryDevice = Marshal.GetDelegateForFunctionPointer<query_device>(_context[0].Ops.query_device);

            Console.WriteLine($"Device params-------------");
            Console.WriteLine(_queryDevice.ToString());
            
            //_mappedRegion = MMap(IntPtr.Zero, (ulong)_buffers * (ulong)_bufferSize, MemoryMappedProtections.PROT_READ | MemoryMappedProtections.PROT_WRITE, MemoryMappedFlags.MAP_PRIVATE | MemoryMappedFlags.MAP_ANONYMOUS, new FileDescriptor(), 0);

        }

        private unsafe Interop.IbvContext.ibv_context* OpenContext(string deviceName)
        {
            var devices = Interop.IbvDevice.ibv_get_device_list(out var numberOfDevices);
            try
            {
                Console.WriteLine($"Number of devices found {numberOfDevices}");
                for (var i = 0; i < numberOfDevices; i++)
                {
                    if (deviceName == devices[0][i].Name)
                    {
                        Console.WriteLine("Found matching device");
                        var context = ibv_open_device(ref devices[0][i]);
                        Console.WriteLine($"Device opened - {context[0].Device[0].ToString()}");
                        return context;
                    }
                }
                throw new InvalidOperationException("Couldn't find device");
            }
            finally
            {
                IbvDevice.ibv_free_device_list(devices);
            }
        }
    }
}

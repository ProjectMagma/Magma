using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Magma.Network.Abstractions;
using Magma.Network.Header;
using Microsoft.Win32;
using Microsoft.Win32.SafeHandles;
using static Magma.WinTun.Interop.WinIO;

namespace Magma.WinTun.Internal
{
    public class WinTunPort<TPacketReceiver> : IDisposable where TPacketReceiver : IPacketReceiver
    {
        private const string UsermodeDeviceSpace = "\\\\.\\Global\\";
        private const string AdapterKey = "SYSTEM\\CurrentControlSet\\Control\\Class\\{4D36E972-E325-11CE-BFC1-08002BE10318}";
        private const string ConnectionKey = "SYSTEM\\CurrentControlSet\\Control\\Network\\{4D36E972-E325-11CE-BFC1-08002BE10318}";

        private readonly string _deviceGuid;
        private readonly SafeFileHandle _fileHandle;
        private readonly FileStream _fileStream;
        private readonly WinTunMemoryPool _pool;
        private readonly TPacketReceiver _packetReceiver;
        private readonly WinTunTransitter _transmitter;

        public WinTunPort(string adapterName, Func<WinTunTransitter, TPacketReceiver> packetReceiverFactory)
        {   
            _pool = new WinTunMemoryPool(1000, 2000);
            _deviceGuid = GetGuidForName(adapterName);
            _fileHandle = CreateFile($"{UsermodeDeviceSpace}{_deviceGuid}.tap", FileAccess.ReadWrite, FileShare.ReadWrite, 0, FileMode.Open, FILE_ATTRIBUTE_SYSTEM | FILE_FLAG_OVERLAPPED, IntPtr.Zero);
            SetMediaStatus(_fileHandle);
            SetTapIOCtl(_fileHandle);
            _fileStream = new FileStream(_fileHandle, FileAccess.ReadWrite, 2000, isAsync: true);
            _transmitter = new WinTunTransitter(_fileStream, _pool);
            _packetReceiver = packetReceiverFactory(_transmitter);
            var ignore = ReadLoop();
        }

        private async Task ReadLoop()
        {
            while (true)
            {
                var ownedMemory = await _pool.GetMemoryAsync();
                var result = await _fileStream.ReadAsync(ownedMemory.Memory);
                TestIpV4(ownedMemory.Memory.Slice(0, result));

                if (_packetReceiver.TryConsume(ownedMemory) == default) ownedMemory.Return();
            }
        }

        private void TestIpV4(Memory<byte> input)
        {
            var eth = Ethernet.TryConsume(input.Span, out var ethernet, out var data);
            var ipv4 = IPv4.TryConsume(input.Span, out var ip, out data);
        }

        private string GetGuidForName(string adapaterName)
        {
            foreach (var device in GetDeviceGuids())
            {
                var name = GetAdapterName(device);
                if (StringComparer.OrdinalIgnoreCase.Compare(name, adapaterName) == 0)
                {
                    return device;
                }
            }
            throw new InvalidOperationException("Unknown name!");
        }

        private string GetAdapterName(string adapterGuid)
        {
            if (string.IsNullOrWhiteSpace(adapterGuid)) return null;
            using (var regKey = Registry.LocalMachine.OpenSubKey($"{ConnectionKey}\\{adapterGuid}\\Connection", writable: false))
            {
                var name = regKey.GetValue("Name");
                return name?.ToString();
            }
        }

        private IEnumerable<string> GetDeviceGuids()
        {
            using (var adapters = Registry.LocalMachine.OpenSubKey(AdapterKey, writable: false))
            {
                foreach (var adapterName in adapters.GetSubKeyNames())
                {
                    if (!int.TryParse(adapterName, out _)) continue;
                    using (var adapter = adapters.OpenSubKey(adapterName, writable: false))
                    {
                        yield return adapter.GetValue("NetCfgInstanceId").ToString();
                    }
                }
            }
        }

        public void Dispose() => _fileHandle.Dispose();
    }
}

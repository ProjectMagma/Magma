using System;
using System.Runtime.InteropServices;
using Magma.Network.Abstractions;
using Magma.WinTun.Internal;
using Xunit;

namespace Magma.WinTun.Facts
{
    [Trait("Category", "Integration")]
    public class WinTunPortIntegrationFacts
    {
        private static bool IsWindows => RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
        private static bool IntegrationTestsEnabled => 
            !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("WINTUN_INTEGRATION_TESTS"));

        [Fact(Skip = "Requires WinTun driver and adapter - set WINTUN_INTEGRATION_TESTS=1 to enable")]
        public void CanCreateWinTunPort()
        {
            if (!IsWindows || !IntegrationTestsEnabled)
            {
                return;
            }

            var adapterName = Environment.GetEnvironmentVariable("WINTUN_ADAPTER_NAME") ?? "WinTun";
            
            using (var port = new WinTunPort<TestPacketReceiver>(
                adapterName, 
                transmitter => new TestPacketReceiver()))
            {
                Assert.NotNull(port);
            }
        }

        [Fact(Skip = "Requires WinTun driver and adapter - set WINTUN_INTEGRATION_TESTS=1 to enable")]
        public void WinTunPortDisposesCleanly()
        {
            if (!IsWindows || !IntegrationTestsEnabled)
            {
                return;
            }

            var adapterName = Environment.GetEnvironmentVariable("WINTUN_ADAPTER_NAME") ?? "WinTun";
            
            var port = new WinTunPort<TestPacketReceiver>(
                adapterName, 
                transmitter => new TestPacketReceiver());
            
            port.Dispose();
            
            port.Dispose();
        }

        private class TestPacketReceiver : IPacketReceiver
        {
            public void FlushPendingAcks()
            {
            }

            public T TryConsume<T>(T input) where T : System.Buffers.IMemoryOwner<byte>
            {
                return default;
            }
        }
    }
}

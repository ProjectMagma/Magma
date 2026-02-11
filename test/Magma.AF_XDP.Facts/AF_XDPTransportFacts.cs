using System;
using System.Net;
using System.Threading.Tasks;
using Magma.AF_XDP;
using Microsoft.AspNetCore.Server.Kestrel.Transport.Abstractions.Internal;
using Xunit;

namespace Magma.AF_XDP.Facts
{
    public class AF_XDPTransportFacts
    {
        [Fact]
        public void CanCreateTransportWithEndpoint()
        {
            var endpoint = new IPEndPoint(IPAddress.Parse("192.168.1.1"), 8080);
            var options = new AF_XDPTransportOptions { InterfaceName = "eth0" };
            var dispatcher = new MockConnectionDispatcher();

            var transport = new AF_XDPTransport(endpoint, options, dispatcher);

            Assert.NotNull(transport);
        }

        [Fact]
        public void ConstructorThrowsOnNullEndpoint()
        {
            var options = new AF_XDPTransportOptions { InterfaceName = "eth0" };
            var dispatcher = new MockConnectionDispatcher();

            Assert.Throws<ArgumentNullException>(() =>
                new AF_XDPTransport(null, options, dispatcher));
        }

        [Fact]
        public void ConstructorThrowsOnNullOptions()
        {
            var endpoint = new IPEndPoint(IPAddress.Parse("192.168.1.1"), 8080);
            var dispatcher = new MockConnectionDispatcher();

            Assert.Throws<ArgumentNullException>(() =>
                new AF_XDPTransport(endpoint, (AF_XDPTransportOptions)null, dispatcher));
        }

        [Fact]
        public void ConstructorThrowsOnNullDispatcher()
        {
            var endpoint = new IPEndPoint(IPAddress.Parse("192.168.1.1"), 8080);
            var options = new AF_XDPTransportOptions { InterfaceName = "eth0" };

            Assert.Throws<ArgumentNullException>(() =>
                new AF_XDPTransport(endpoint, options, null));
        }

        [Fact]
        public void CanCreateTransportWithInterfaceName()
        {
            var endpoint = new IPEndPoint(IPAddress.Parse("192.168.1.1"), 8080);
            var dispatcher = new MockConnectionDispatcher();

            var transport = new AF_XDPTransport(endpoint, "eth0", dispatcher);

            Assert.NotNull(transport);
        }

        [Fact(Skip = "Requires Linux with XDP-capable NIC and root privileges")]
        public async Task CanBindTransport()
        {
            var endpoint = new IPEndPoint(IPAddress.Parse("192.168.1.1"), 8080);
            var options = new AF_XDPTransportOptions { InterfaceName = "eth0" };
            var dispatcher = new MockConnectionDispatcher();

            var transport = new AF_XDPTransport(endpoint, options, dispatcher);

            await transport.BindAsync();
            await transport.StopAsync();
        }

        [Fact(Skip = "Requires Linux with XDP-capable NIC and root privileges")]
        public async Task CanUnbindTransport()
        {
            var endpoint = new IPEndPoint(IPAddress.Parse("192.168.1.1"), 8080);
            var options = new AF_XDPTransportOptions { InterfaceName = "eth0" };
            var dispatcher = new MockConnectionDispatcher();

            var transport = new AF_XDPTransport(endpoint, options, dispatcher);

            await transport.BindAsync();
            await transport.UnbindAsync();
            await transport.StopAsync();
        }

        private class MockConnectionDispatcher : IConnectionDispatcher
        {
            public void OnConnection(TransportConnection connection)
            {
            }
        }
    }
}

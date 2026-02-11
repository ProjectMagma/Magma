using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Magma.AF_XDP.Internal;
using Magma.Network.Abstractions;
using Magma.Transport.Tcp;
using Microsoft.AspNetCore.Server.Kestrel.Transport.Abstractions.Internal;

namespace Magma.AF_XDP
{
    /// <summary>
    /// AF_XDP (XDP socket) transport implementation for high-performance packet I/O on Linux.
    /// Requires Linux kernel 4.18+ with XDP support.
    /// </summary>
    public class AF_XDPTransport : ITransport
    {
        private AF_XDPPort<TcpTransportReceiver<AF_XDPTransmitRing>> _port;
        private IPEndPoint _endpoint;
        private string _interfaceName;
        private IConnectionDispatcher _connectionDispatcher;
        private List<TcpTransportReceiver<AF_XDPTransmitRing>> _receivers = new List<TcpTransportReceiver<AF_XDPTransmitRing>>();

        public AF_XDPTransport(IPEndPoint ipEndpoint, string interfaceName, IConnectionDispatcher dispatcher)
        {
            _endpoint = ipEndpoint ?? throw new ArgumentNullException(nameof(ipEndpoint));
            _interfaceName = interfaceName ?? throw new ArgumentNullException(nameof(interfaceName));
            _connectionDispatcher = dispatcher ?? throw new ArgumentNullException(nameof(dispatcher));
            Console.WriteLine($"AF_XDP Transport started with IP endpoint {ipEndpoint} on interface {interfaceName}");
        }

        public Task BindAsync()
        {
            _port = new AF_XDPPort<TcpTransportReceiver<AF_XDPTransmitRing>>(_interfaceName, CreateReceiver);
            _port.Open();
            Console.WriteLine($"AF_XDP port opened and bound to {_interfaceName}");
            return Task.CompletedTask;
        }

        private TcpTransportReceiver<AF_XDPTransmitRing> CreateReceiver(AF_XDPTransmitRing transmitRing)
        {
            var receiver = new TcpTransportReceiver<AF_XDPTransmitRing>(_endpoint, transmitRing, _connectionDispatcher);
            _receivers.Add(receiver);
            Console.WriteLine("AF_XDP receiver created");
            return receiver;
        }

        public Task StopAsync()
        {
            _port?.Dispose();
            _port = null;
            return Task.CompletedTask;
        }

        public Task UnbindAsync() => Task.CompletedTask;
    }
}

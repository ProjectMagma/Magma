using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Magma.NetMap.Internal;
using Magma.Network.Abstractions;
using Magma.Transport.Tcp;
using Microsoft.AspNetCore.Server.Kestrel.Transport.Abstractions.Internal;

namespace Magma.NetMap
{
    public class NetMapTransport : ITransport
    {
        private NetMapPort<TcpTransportReceiver<NetMapTransmitRing>> _port;
        private IPEndPoint _endpoint;
        private string _interfaceName;
        private IConnectionDispatcher _connectionDispatcher;
        private List<TcpTransportReceiver<NetMapTransmitRing>> _receivers = new List<TcpTransportReceiver<NetMapTransmitRing>>();

        public NetMapTransport(IPEndPoint ipEndpoint, string interfaceName, IConnectionDispatcher dispatcher)
        {
            _endpoint = ipEndpoint ?? throw new ArgumentNullException(nameof(ipEndpoint));
            _interfaceName = interfaceName ?? throw new ArgumentNullException(nameof(interfaceName));
            _connectionDispatcher = dispatcher ?? throw new ArgumentNullException(nameof(dispatcher));
            Console.WriteLine($"Transport started with ip endpoint {ipEndpoint} on interface name {interfaceName}");
        }

        public Task BindAsync()
        {
            _port = new NetMapPort<TcpTransportReceiver<NetMapTransmitRing>>(_interfaceName, CreateReceiver);
            _port.Open();
            Console.WriteLine($"Bind completed and netmap port open");
            return Task.CompletedTask;
        }

        private TcpTransportReceiver<NetMapTransmitRing> CreateReceiver(NetMapTransmitRing transmitRing)
        {
            var receiver = new TcpTransportReceiver<NetMapTransmitRing>(_endpoint, transmitRing, _connectionDispatcher);
            _receivers.Add(receiver);
            Console.WriteLine("Creating receiver");
            return receiver;
        }

        public Task StopAsync()
        {
            _port.Dispose();
            _port = null;
            return Task.CompletedTask;
        }

        public Task UnbindAsync() => Task.CompletedTask;
    }
}

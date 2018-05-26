using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Server.Kestrel.Transport.Abstractions.Internal;

namespace Magma.NetMap.TcpHost
{
    public class NetMapTransport : ITransport
    {
        private NetMapPort<TcpReceiver> _port;
        private IPEndPoint _endpoint;
        private string _interfaceName;
        private IConnectionDispatcher _connectionDispatcher;
        private List<TcpReceiver> _receivers = new List<TcpReceiver>();

        public NetMapTransport(IPEndPoint ipEndpoint, string interfaceName, IConnectionDispatcher dispatcher)
        {
            _endpoint = ipEndpoint ?? throw new ArgumentNullException(nameof(ipEndpoint));
            _interfaceName = interfaceName ?? throw new ArgumentNullException(nameof(interfaceName));
            _connectionDispatcher = dispatcher ?? throw new ArgumentNullException(nameof(dispatcher));
        }

        public Task BindAsync()
        {
            _port = new NetMapPort<TcpReceiver>(_interfaceName, CreateReceiver);
            _port.Open();
            return Task.CompletedTask;
        }

        private TcpReceiver CreateReceiver(NetMapTransmitRing transmitRing)
        {
            var receiver = new TcpReceiver(_endpoint, transmitRing, _connectionDispatcher);
            _receivers.Add(receiver);
            return receiver;
        }

        public Task StopAsync()
        {
            throw new NotImplementedException();
        }

        public Task UnbindAsync()
        {
            throw new NotImplementedException();
        }
    }
}

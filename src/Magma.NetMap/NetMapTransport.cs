using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Magma.NetMap.Internal;
using Microsoft.AspNetCore.Server.Kestrel.Transport.Abstractions.Internal;

namespace Magma.NetMap
{
    public class NetMapTransport : ITransport
    {
        private NetMapPort<NetMapTransportReceiver> _port;
        private IPEndPoint _endpoint;
        private string _interfaceName;
        private IConnectionDispatcher _connectionDispatcher;
        private List<NetMapTransportReceiver> _receivers = new List<NetMapTransportReceiver>();
#if TRACE
        private PCap.PCapFileWriter _pcapWriter;
#endif

        public NetMapTransport(IPEndPoint ipEndpoint, string interfaceName, IConnectionDispatcher dispatcher)
        {
#if TRACE
            _pcapWriter = new PCap.PCapFileWriter("networkdata.pcap");
#endif
            _endpoint = ipEndpoint ?? throw new ArgumentNullException(nameof(ipEndpoint));
            _interfaceName = interfaceName ?? throw new ArgumentNullException(nameof(interfaceName));
            _connectionDispatcher = dispatcher ?? throw new ArgumentNullException(nameof(dispatcher));
        }

        public Task BindAsync()
        {
            _port = new NetMapPort<NetMapTransportReceiver>(_interfaceName, CreateReceiver);
            _port.Open();
            return Task.CompletedTask;
        }

        private NetMapTransportReceiver CreateReceiver(NetMapTransmitRing transmitRing)
        {
            var receiver = new NetMapTransportReceiver(_endpoint, transmitRing, _connectionDispatcher, _pcapWriter);
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

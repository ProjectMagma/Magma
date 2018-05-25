using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Server.Kestrel.Transport.Abstractions.Internal;
using static Magma.Network.IPAddress;

namespace Magma.Transport.Tcp
{
    public class TcpTransport : ITransport
    {
        private IConnectionDispatcher _connectionDispatcher;
        private V4Address _listeningAddress;
        private ushort _listeningPort;
        private Dictionary<(V4Address address, uint port), object> _activeConnections = new Dictionary<(V4Address, uint), object>();

        public TcpTransport(IConnectionDispatcher dispatcher)
        {
            _connectionDispatcher = dispatcher;
        }
               

        public Task BindAsync()
        {
            throw new NotImplementedException();
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

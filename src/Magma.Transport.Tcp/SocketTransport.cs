using System;
using System.Buffers;
using System.Threading.Tasks;
using Magma.Transport.Tcp.Internal;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Transport.Abstractions.Internal;

namespace Magma.Transport.Tcp
{
    internal sealed class SocketTransport : ITransport
    {
        private IEndPointInformation _endPointInformation;
        private IConnectionDispatcher _dispatcher;
        private IApplicationLifetime _appLifetime;
        private int _iOQueueCount;
        private SocketsTrace _trace;
        private MemoryPool<byte> _memoryPool;

        public SocketTransport(IEndPointInformation endPointInformation, IConnectionDispatcher dispatcher, IApplicationLifetime appLifetime, int iOQueueCount, SocketsTrace trace, MemoryPool<byte> memoryPool)
        {
            _endPointInformation = endPointInformation;
            _dispatcher = dispatcher;
            _appLifetime = appLifetime;
            _iOQueueCount = iOQueueCount;
            _trace = trace;
            _memoryPool = memoryPool;
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

using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Transport.Abstractions.Internal;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Magma.Transport.Tcp.Internal;

namespace Magma.Transport.Tcp
{
    public sealed class SocketTransportFactory : ITransportFactory
    {
        private readonly SocketTransportOptions _options;
        private readonly IApplicationLifetime _appLifetime;
        private readonly SocketsTrace _trace;

        public SocketTransportFactory(
            IOptions<SocketTransportOptions> options,
            IApplicationLifetime applicationLifetime,
            ILoggerFactory loggerFactory)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            if (loggerFactory == null)
            {
                throw new ArgumentNullException(nameof(loggerFactory));
            }

            _options = options.Value;
            _appLifetime = applicationLifetime ?? throw new ArgumentNullException(nameof(applicationLifetime));
            var logger = loggerFactory.CreateLogger("Magma.Transport.Tcp.Sockets");
            _trace = new SocketsTrace(logger);
        }

        public ITransport Create(IEndPointInformation endPointInformation, IConnectionDispatcher dispatcher)
        {
            if (endPointInformation == null)
            {
                throw new ArgumentNullException(nameof(endPointInformation));
            }

            if (endPointInformation.Type != ListenType.IPEndPoint)
            {
                throw new ArgumentException(SocketsStrings.OnlyIPEndPointsSupported, nameof(endPointInformation));
            }

            if (dispatcher == null)
            {
                throw new ArgumentNullException(nameof(dispatcher));
            }

            throw new NotImplementedException();
            //return new SocketTransport(endPointInformation, dispatcher, _appLifetime, _options.IOQueueCount, _trace, _options.MemoryPoolFactory());
        }
    }
}

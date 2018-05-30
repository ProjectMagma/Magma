using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Transport.Abstractions.Internal;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Magma.NetMap
{
    public class NetMapTransportFactory : ITransportFactory
    {
        private readonly NetMapTransportOptions _options;
        private readonly IApplicationLifetime _appLifetime;

        public NetMapTransportFactory(IOptions<NetMapTransportOptions> options, IApplicationLifetime applicationLifetime)
        {
            _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
            _appLifetime = applicationLifetime ?? throw new ArgumentNullException(nameof(applicationLifetime));
        }

        public ITransport Create(IEndPointInformation endPointInformation, IConnectionDispatcher dispatcher)
        {
            endPointInformation = endPointInformation ?? throw new ArgumentNullException(nameof(endPointInformation));
            dispatcher = dispatcher ?? throw new ArgumentNullException(nameof(dispatcher));

            var transport = new NetMapTransport(endPointInformation.IPEndPoint, _options.InterfaceName, dispatcher);
            return transport;
        }
    }
}

using System;
using Magma.NetMap;
using Microsoft.AspNetCore.Server.Kestrel.Transport.Abstractions.Internal;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.AspNetCore.Hosting
{
    public static class WebHostBuilderNetMapExtensions
    {
        /// <summary>
        /// Specify NetMap as the transport to be used by Kestrel.
        /// </summary>
        /// <param name="hostBuilder">
        /// The Microsoft.AspNetCore.Hosting.IWebHostBuilder to configure.
        /// </param>
        /// <returns>
        /// The Microsoft.AspNetCore.Hosting.IWebHostBuilder.
        /// </returns>
        public static IWebHostBuilder UseNetMap(this IWebHostBuilder hostBuilder)
        {
            return hostBuilder.ConfigureServices(services =>
            {
                services.AddSingleton<ITransportFactory, NetMapTransportFactory>();
            });
        }

        /// <summary>
        /// Specify NetMap as the transport to be used by Kestrel.
        /// </summary>
        /// <param name="hostBuilder">
        /// The Microsoft.AspNetCore.Hosting.IWebHostBuilder to configure.
        /// </param>
        /// <param name="configureOptions">
        /// A callback to configure NetMap options.
        /// </param>
        /// <returns>
        /// The Microsoft.AspNetCore.Hosting.IWebHostBuilder.
        /// </returns>
        public static IWebHostBuilder UseNetMap(this IWebHostBuilder hostBuilder, Action<NetMapTransportOptions> configureOptions)
        {
            return hostBuilder.UseNetMap().ConfigureServices(services =>
            {
                services.Configure(configureOptions);
            });
        }
    }
}

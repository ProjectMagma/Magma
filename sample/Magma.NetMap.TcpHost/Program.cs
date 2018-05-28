using System;
using System.Net;
using System.Threading.Tasks;

namespace Magma.NetMap.TcpHost
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var dispatcher = new TestConnectionDispatcher();
            var transport = new NetMapTransport(new IPEndPoint(IPAddress.Any, 7000), "eth0", dispatcher);
            await transport.BindAsync();
            Console.WriteLine("Hello World!");
            Console.ReadLine();
        }
    }
}

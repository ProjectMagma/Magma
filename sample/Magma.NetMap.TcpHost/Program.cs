using System;
using System.Net;

namespace Magma.NetMap.TcpHost
{
    class Program
    {
        static void Main(string[] args)
        {
            var dispatcher = new TestConnectionDispatcher();
            var transport = new NetMapTransport(new IPEndPoint(IPAddress.Any, 6667), "eth0", dispatcher);

            Console.WriteLine("Hello World!");
        }
    }
}

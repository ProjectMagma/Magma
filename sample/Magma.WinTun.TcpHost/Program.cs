using System;
using System.Net;
using Magma.Transport.Tcp;
using Magma.WinTun.Internal;

namespace Magma.WinTun.TcpHost
{
    class Program
    {
        static void Main(string[] args)
        {
            var ipEndPoint = new IPEndPoint(IPAddress.Parse("192.168.10.7"), 5001);
            var winPort = new WinTunPort<TcpTransportReceiver<WinTunTransitter>>("TunTest", t => new TcpTransportReceiver<WinTunTransitter>(ipEndPoint, t, null));
            Console.ReadLine();
        }
    }
}

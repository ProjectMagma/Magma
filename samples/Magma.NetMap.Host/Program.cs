using System;
using Magma.NetMap.Interop;

namespace Magma.NetMap.Host
{
    class Program
    {
        static unsafe void Main(string[] args)
        {
            var netmap = new NetMapInterop();
            netmap.Open("eth0");

            Console.WriteLine("Hello World!");
        }
    }
}

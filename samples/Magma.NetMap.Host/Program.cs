using System;
using Magma.NetMap.Interop;

namespace Magma.NetMap.Host
{
    class Program
    {
        static unsafe void Main(string[] args)
        {
            var netmap = new NetMapPort("eth0");
            netmap.Open();
            netmap.PrintPortInfo();

            Console.WriteLine("Hello World!");
        }
    }
}

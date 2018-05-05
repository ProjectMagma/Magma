using System;
using Magma.NetMap.Interop;

namespace Magma.NetMap.Host
{
    class Program
    {
        static unsafe void Main(string[] args)
        {
            NetMap.Interop.NetMapInterop.OpenNetMap("eth0");
            Console.WriteLine("Hello World!");
        }
    }
}

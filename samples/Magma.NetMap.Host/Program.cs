using System;
using Magma.NetMap.Interop;

namespace Magma.NetMap.Host
{
    class Program
    {
        static unsafe void Main(string[] args)
        {
            var desc = NetMap.Interop.NetMapInterop.nm_open("eth0", default(nmreq), 0, null);
            Console.WriteLine("Hello World!");
        }
    }
}

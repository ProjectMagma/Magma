using System;

namespace Magma.Infiniband.RdmaHost
{
    class Program
    {
        static void Main(string[] args)
        {
            var port = new InfinibandPort(1024, 1024, "mlx4_0");
            Console.WriteLine("Press any key to exit");
            Console.Read();
        }
    }
}

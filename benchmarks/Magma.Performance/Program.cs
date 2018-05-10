using System;
using System.Reflection;
using BenchmarkDotNet.Running;

namespace Magma.Performance
{
    [DefaultBenchmark]
    public class Program
    {
        static void Main(string[] args)
        {
            BenchmarkSwitcher.FromAssembly(typeof(Program).GetTypeInfo().Assembly)
                .Run(args, new DefaultCoreConfig());
        }
    }
}

using System;
using BenchmarkDotNet.Configs;

namespace Magma.Performance
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Assembly)]
    internal class DefaultBenchmarkAttribute : Attribute, IConfigSource
    {
        public DefaultBenchmarkAttribute()
        {
        }

        public IConfig Config => new DefaultCoreConfig();
    }
}

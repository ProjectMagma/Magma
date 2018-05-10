// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Engines;
using BenchmarkDotNet.Exporters;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Loggers;
using BenchmarkDotNet.Toolchains.CsProj;
using BenchmarkDotNet.Toolchains.DotNetCli;
using BenchmarkDotNet.Validators;

namespace Magma.Performance
{
    public class DefaultCoreConfig : ManualConfig
    {
        public DefaultCoreConfig()
        {
            Add(MarkdownExporter.GitHub);

            Add(MemoryDiagnoser.Default);
            Add(StatisticColumn.OperationsPerSecond);
            Add(DefaultColumnProviders.Job);
            Add(DefaultColumnProviders.Instance);
            Add(DefaultColumnProviders.Params);
            Add(DefaultColumnProviders.Diagnosers);

            Add(StatisticColumn.Mean);
            Add(StatisticColumn.Median);

            Add(StatisticColumn.StdErr);

            Add(BaselineScaledColumn.Scaled);

            Add(ConsoleLogger.Default);


            Add(JitOptimizationsValidator.FailOnError);

            Add(Job.Core
                .With(CsProjCoreToolchain.From(NetCoreAppSettings.NetCoreApp21))
                .With(new GcMode { Server = true })
                .With(RunStrategy.Throughput));
        }
        
    }
}

// Copyright (c) 2022 Jonathan Lang

using System.Threading;
using Baracuda.Monitoring.API;

namespace Baracuda.Monitoring.Source.Interfaces
{
    internal interface IMonitoringProfiler : IMonitoringSubsystem<IMonitoringProfiler>
    {
        void BeginProfiling(CancellationToken ct);
    }
}


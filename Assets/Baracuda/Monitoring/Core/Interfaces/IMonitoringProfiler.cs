// Copyright (c) 2022 Jonathan Lang

using System.Threading;

namespace Baracuda.Monitoring.Interfaces
{
    internal interface IMonitoringProfiler : IMonitoringSubsystem<IMonitoringProfiler>
    {
        void BeginProfiling(CancellationToken ct);
    }
}


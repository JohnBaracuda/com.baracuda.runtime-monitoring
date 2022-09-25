// Copyright (c) 2022 Jonathan Lang

using Baracuda.Monitoring.API;
using System.Threading;

namespace Baracuda.Monitoring.Interfaces
{
    internal interface IMonitoringProfiler : IMonitoringSubsystem<IMonitoringProfiler>
    {
        void BeginProfiling(CancellationToken ct);
    }
}


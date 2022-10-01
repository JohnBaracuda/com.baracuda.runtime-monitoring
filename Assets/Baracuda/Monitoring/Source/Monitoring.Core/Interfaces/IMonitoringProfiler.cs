// Copyright (c) 2022 Jonathan Lang

using Baracuda.Monitoring.Interfaces;
using System.Threading;

namespace Baracuda.Monitoring.Core.Interfaces
{
    internal interface IMonitoringProfiler : IMonitoringSubsystem<IMonitoringProfiler>
    {
        void BeginProfiling(CancellationToken ct);
    }
}


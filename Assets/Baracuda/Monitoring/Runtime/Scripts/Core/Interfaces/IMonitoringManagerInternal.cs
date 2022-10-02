// Copyright (c) 2022 Jonathan Lang

using Baracuda.Monitoring.Core.Profiles;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Baracuda.Monitoring.Core.Interfaces
{
    internal interface IMonitoringManagerInternal : IMonitoringSubsystem<IMonitoringManagerInternal>
    {
        Task CompleteProfilingAsync(List<MonitorProfile> staticProfiles, Dictionary<Type, List<MonitorProfile>> instanceProfiles, CancellationToken ct);
    }
}
// Copyright (c) 2022 Jonathan Lang

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Baracuda.Monitoring.API;
using Baracuda.Monitoring.Source.Profiles;

namespace Baracuda.Monitoring.Source.Interfaces
{
    internal interface IMonitoringManagerInternal : IMonitoringSubsystem<IMonitoringManagerInternal>
    {
        Task CompleteProfilingAsync(List<MonitorProfile> staticProfiles, Dictionary<Type, List<MonitorProfile>> instanceProfiles, CancellationToken ct);
    }
}
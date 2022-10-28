// Copyright (c) 2022 Jonathan Lang

using System;
using System.Collections.Generic;

namespace Baracuda.Monitoring
{
    [Obsolete("Use Monitor, MonitoringEvents and MonitoringRegistry instead! This API will be removed in 4.0.0")]
    public interface IMonitoringManager : IMonitoringSubsystem<IMonitoringManager>
    {
        [Obsolete("Use Monitor.Initialized instead! This API will be removed in 4.0.0")]
        bool IsInitialized { get; }

        [Obsolete("Use Monitor.Events.ProfilingCompleted instead! This API will be removed in 4.0.0")]
        event ProfilingCompletedListener ProfilingCompleted;

        [Obsolete("Use Monitor.Events.MonitorHandleCreated instead! This API will be removed in 4.0.0")]
        event Action<IMonitorHandle> UnitCreated;

        [Obsolete("Use Monitor.Events.MonitorHandleDisposed instead! This API will be removed in 4.0.0")]
        event Action<IMonitorHandle> UnitDisposed;

        [Obsolete("Use Monitor.StartMonitoring instead! This API will be removed in 4.0.0")]
        void RegisterTarget<T>(T target) where T : class;

        [Obsolete("Use Monitor.StopMonitoring instead! This API will be removed in 4.0.0")]
        void UnregisterTarget<T>(T target) where T : class;

        [Obsolete("Use Monitor.Registry.GetMonitorHandles() instead! This API will be removed in 4.0.0")]
        IReadOnlyList<IMonitorHandle> GetStaticUnits();

        [Obsolete("Use Monitor.Registry.GetMonitorHandles() instead! This API will be removed in 4.0.0")]
        IReadOnlyList<IMonitorHandle> GetInstanceUnits();

        [Obsolete("Use Monitor.Registry.GetMonitorHandles() instead! This API will be removed in 4.0.0")]
        IReadOnlyList<IMonitorHandle> GetAllMonitoringUnits();
    }
}
// Copyright (c) 2022 Jonathan Lang

using System;
using System.Collections.Generic;

namespace Baracuda.Monitoring.Systems
{
    [Obsolete]
    internal class MonitoringManager : IMonitoringManager
    {
        [Obsolete]
        public bool IsInitialized => Monitor.Initialized;

        [Obsolete]
        public event ProfilingCompletedListener ProfilingCompleted
        {
            add => Monitor.Events.__ProfilingCompleted += value;
            remove => Monitor.Events.__ProfilingCompleted -= value;
        }

        [Obsolete]
        public event Action<IMonitorHandle> UnitCreated
        {
            add => Monitor.Events.MonitorHandleCreated += value;
            remove => Monitor.Events.MonitorHandleCreated -= value;
        }

        [Obsolete]
        public event Action<IMonitorHandle> UnitDisposed
        {
            add => Monitor.Events.MonitorHandleDisposed += value;
            remove => Monitor.Events.MonitorHandleDisposed -= value;
        }

        [Obsolete]
        public void RegisterTarget<T>(T target) where T : class
        {
            Monitor.StartMonitoring(target);
        }

        [Obsolete]
        public void UnregisterTarget<T>(T target) where T : class
        {
            Monitor.StopMonitoring(target);
        }

        [Obsolete]
        public IReadOnlyList<IMonitorHandle> GetStaticUnits()
        {
            return Monitor.Registry.GetMonitorHandles(HandleTypes.Static);
        }

        [Obsolete]
        public IReadOnlyList<IMonitorHandle> GetInstanceUnits()
        {
            return Monitor.Registry.GetMonitorHandles(HandleTypes.Instance);
        }

        [Obsolete]
        public IReadOnlyList<IMonitorHandle> GetAllMonitoringUnits()
        {
            return Monitor.Registry.GetMonitorHandles();
        }
    }
}
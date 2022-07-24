// Copyright (c) 2022 Jonathan Lang
using System;
using Baracuda.Monitoring.API;

namespace Baracuda.Monitoring
{
    public abstract class MonitoredObject : IDisposable
    {
        protected MonitoredObject()
        {
            MonitoringSystems.Resolve<IMonitoringManager>().RegisterTarget(this);
        }

        public virtual void Dispose()
        {
            MonitoringSystems.Resolve<IMonitoringManager>().UnregisterTarget(this);
        }
    }
}
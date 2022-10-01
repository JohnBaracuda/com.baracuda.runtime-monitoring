// Copyright (c) 2022 Jonathan Lang
using System;

namespace Baracuda.Monitoring
{
    public abstract class MonitoredObject : IDisposable
    {
        protected MonitoredObject()
        {
            MonitoringSystems.MonitoringManager.RegisterTarget(this);
        }

        public virtual void Dispose()
        {
            MonitoringSystems.MonitoringManager.UnregisterTarget(this);
        }
    }
}
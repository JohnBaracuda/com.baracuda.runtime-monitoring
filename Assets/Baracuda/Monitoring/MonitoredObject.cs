// Copyright (c) 2022 Jonathan Lang
using System;
using Baracuda.Monitoring.API;

namespace Baracuda.Monitoring
{
    public abstract class MonitoredObject : IDisposable
    {
        protected MonitoredObject()
        {
            MonitoringManager.RegisterTarget(this);
        }

        public virtual void Dispose()
        {
            MonitoringManager.UnregisterTarget(this);
        }
    }
}
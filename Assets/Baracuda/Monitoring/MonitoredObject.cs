using System;
using Baracuda.Monitoring.API;

namespace Baracuda.Monitoring
{
    public abstract class MonitoredObject : IDisposable
    {
        protected MonitoredObject()
        {
            MonitoringUnitManager.RegisterTarget(this);
        }

        public virtual void Dispose()
        {
            MonitoringUnitManager.UnregisterTarget(this);
        }
    }
}
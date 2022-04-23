using System;
using Baracuda.Monitoring.Management;

namespace Baracuda.Monitoring
{
    public abstract class MonitoredObject : IDisposable
    {
        private bool _isDisposed = false;
        
        protected MonitoredObject()
        {
            MonitoringManager.RegisterTarget(this);
        }
        
        ~ MonitoredObject()
        {
            Dispose();
        }

        public virtual void Dispose()
        {
            if (_isDisposed)
            {
                return;
            }

            MonitoringManager.UnregisterTarget(this);
            _isDisposed = true;
            GC.SuppressFinalize(this);
        }
    }
}
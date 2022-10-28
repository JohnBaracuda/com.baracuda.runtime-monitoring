// Copyright (c) 2022 Jonathan Lang

using System;

namespace Baracuda.Monitoring
{
    /// <summary>
    /// Base class for monitored objects.
    /// </summary>
    public abstract class MonitoredObject : object, IDisposable
    {
        /// <summary>
        /// Base class for monitored objects.
        /// </summary>
        protected MonitoredObject()
        {
            Monitor.StartMonitoring(this);
        }

        /// <summary>
        /// Ensure to call base.Dispose when overriding this method.
        /// </summary>
        public virtual void Dispose()
        {
            Monitor.StopMonitoring(this);
        }
    }
}
using System;

namespace Baracuda.Monitoring
{
    public interface IMonitoringEvents
    {
        /// <summary>
        /// Event is invoked when profiling process for the current system has been completed.
        /// Subscribing to this event will instantly invoke a callback if profiling has already completed.
        /// </summary>
        event ProfilingCompletedListener ProfilingCompleted;

        /// <summary>
        /// Event is called when a new <see cref="IMonitorHandle"/> was created.
        /// </summary>
        event Action<IMonitorHandle> MonitorHandleCreated;

        /// <summary>
        /// Event is called when a <see cref="IMonitorHandle"/> was disposed.
        /// </summary>
        event Action<IMonitorHandle> MonitorHandleDisposed;
    }
}
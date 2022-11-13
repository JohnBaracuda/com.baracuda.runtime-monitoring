using Baracuda.Monitoring.Types;
using System;
using System.Collections.Generic;

namespace Baracuda.Monitoring
{
    internal class MonitoringEvents : IMonitoringEvents
    {
        /// <summary>
        /// Event is invoked when profiling process for the current system has been completed.
        /// Subscribing to this event will instantly invoke a callback if profiling has already completed.
        /// </summary>
        public event ProfilingCompletedDelegate ProfilingCompleted
        {
            add
            {
                if (Monitor.Initialized)
                {
                    var staticHandleList = Monitor.Registry.GetMonitorHandles(HandleTypes.Static);
                    var instanceHandleList = Monitor.Registry.GetMonitorHandles(HandleTypes.Instance);
                    value.Invoke(staticHandleList, instanceHandleList);
                    return;
                }
                _profilingCompleted += value;
            }
            remove => _profilingCompleted -= value;
        }

        private ProfilingCompletedDelegate _profilingCompleted;

        /// <summary>
        /// Event is called when a new <see cref="IMonitorHandle"/> was created.
        /// </summary>
        public event Action<IMonitorHandle> MonitorHandleCreated;

        /// <summary>
        /// Event is called when a <see cref="IMonitorHandle"/> was disposed.
        /// </summary>
        public event Action<IMonitorHandle> MonitorHandleDisposed;


        internal void RaiseProfilingCompleted(IReadOnlyList<IMonitorHandle> staticHandles, IReadOnlyList<IMonitorHandle> instanceHandles)
        {
            _profilingCompleted?.Invoke(staticHandles, instanceHandles);
            _profilingCompleted = null;
        }

        internal void RaiseMonitorHandleCreated(IMonitorHandle handle)
        {
            MonitorHandleCreated?.Invoke(handle);
        }

        internal void RaiseMonitorHandleDisposed(IMonitorHandle handle)
        {
            MonitorHandleDisposed?.Invoke(handle);
        }

        #region Obsolete

#pragma warning disable CS0612
        [Obsolete]
#pragma warning disable CS0618
        public event ProfilingCompletedListener __ProfilingCompleted
#pragma warning restore CS0618
        {
            add
            {
                if (Monitor.Initialized)
                {
                    var staticHandleList = Monitor.Registry.GetMonitorHandles(HandleTypes.Static);
                    var instanceHandleList = Monitor.Registry.GetMonitorHandles(HandleTypes.Instance);

                    var staticList = new List<IMonitorUnit>();
                    var instanceList = new List<IMonitorUnit>();

                    foreach (var monitorHandle in staticHandleList)
                    {
                        staticList.Add(monitorHandle as IMonitorUnit);
                    }

                    foreach (var monitorHandle in instanceHandleList)
                    {
                        instanceList.Add(monitorHandle as IMonitorUnit);
                    }

                    value.Invoke(staticList, instanceList);
                    return;
                }
                __completed += value;
            }
            remove => __completed -= value;
        }

        [Obsolete]
#pragma warning disable CS0618
        private ProfilingCompletedListener __completed;
#pragma warning restore CS0618
#pragma warning restore CS0612

        #endregion
    }
}
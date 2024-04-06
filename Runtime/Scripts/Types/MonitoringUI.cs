using Baracuda.Monitoring.Systems;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Baracuda.Monitoring
{
    public abstract class MonitoringUI : MonitoredBehaviour
    {
        /// <summary>
        /// Ensure to call base.OnEnable when overriding this method.
        /// </summary>
        protected virtual void OnEnable()
        {
            Monitor.UI.SetActiveMonitoringUI(this);
        }

        /// <summary>
        /// Ensure to call base.Start when overriding this method.
        /// </summary>
        protected virtual void Start()
        {
            Monitor.Events.ProfilingCompleted += ManagerOnProfilingCompleted;
        }

        /// <summary>
        /// Ensure to call base.OnDestroy when overriding this method.
        /// </summary>
        protected override void OnDestroy()
        {
            Monitor.Events.ProfilingCompleted -= ManagerOnProfilingCompleted;
            Monitor.Events.MonitorHandleCreated -= OnMonitorHandleCreated;
            Monitor.Events.MonitorHandleDisposed -= OnMonitorHandleDisposed;
            base.OnDestroy();
        }

        private void ManagerOnProfilingCompleted(IReadOnlyList<IMonitorHandle> staticUnits, IReadOnlyList<IMonitorHandle> instanceUnits)
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                return;
            }
#endif

            Monitor.Events.MonitorHandleCreated += OnMonitorHandleCreated;
            Monitor.Events.MonitorHandleDisposed += OnMonitorHandleDisposed;

            for (var i = 0; i < staticUnits.Count; i++)
            {
                OnMonitorHandleCreated(staticUnits[i]);
            }

            for (var i = 0; i < instanceUnits.Count; i++)
            {
                OnMonitorHandleCreated(instanceUnits[i]);
            }

            Visible = Monitor.Settings.OpenDisplayOnLoad;
        }

        /// <summary>
        /// The visible state of the UI.
        /// </summary>
        public abstract bool Visible { get; set; }

        /// <summary>
        /// Use to add UI elements for the passed unit.
        /// </summary>
        protected virtual void OnMonitorHandleCreated(IMonitorHandle handle)
        {
#pragma warning disable CS0618
            OnMonitorUnitCreated(handle as IMonitorUnit);
#pragma warning restore CS0618
        }

        /// <summary>
        /// Use to remove UI elements for the passed unit.
        /// </summary>
        protected virtual void OnMonitorHandleDisposed(IMonitorHandle handle)
        {
#pragma warning disable CS0618
            OnMonitorUnitDisposed(handle as IMonitorUnit);
#pragma warning restore CS0618
        }

        [Obsolete("Use OnMonitorHandleCreated instead! This method will be removed in 4.0.0")]
        protected virtual void OnMonitorUnitCreated(IMonitorUnit handle)
        {
        }

        [Obsolete("Use OnMonitorHandleCreated instead! This method will be removed in 4.0.0")]
        protected virtual void OnMonitorUnitDisposed(IMonitorUnit handle)
        {
        }
    }
}
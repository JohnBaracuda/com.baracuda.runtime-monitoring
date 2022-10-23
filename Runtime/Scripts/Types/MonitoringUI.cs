using Baracuda.Monitoring.Systems;
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
            Monitor.Events.MonitorHandleCreated -= OnMonitorUnitCreated;
            Monitor.Events.MonitorHandleDisposed -= OnMonitorUnitDisposed;
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

            Monitor.Events.MonitorHandleCreated += OnMonitorUnitCreated;
            Monitor.Events.MonitorHandleDisposed += OnMonitorUnitDisposed;

            for (var i = 0; i < staticUnits.Count; i++)
            {
                OnMonitorUnitCreated(staticUnits[i]);
            }

            for (var i = 0; i < instanceUnits.Count; i++)
            {
                OnMonitorUnitCreated(instanceUnits[i]);
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
        protected abstract void OnMonitorUnitCreated(IMonitorHandle handle);

        /// <summary>
        /// Use to remove UI elements for the passed unit.
        /// </summary>
        protected abstract void OnMonitorUnitDisposed(IMonitorHandle handle);
    }
}
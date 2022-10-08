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
            MonitoringSystems.InternalUI.SetActiveMonitoringUI(this);
        }

        /// <summary>
        /// Ensure to call base.Start when overriding this method.
        /// </summary>
        protected virtual void Start()
        {
            MonitoringSystems.Manager.ProfilingCompleted += ManagerOnProfilingCompleted;
        }

        /// <summary>
        /// Ensure to call base.OnDestroy when overriding this method.
        /// </summary>
        protected override void OnDestroy()
        {
            MonitoringSystems.Manager.ProfilingCompleted -= ManagerOnProfilingCompleted;
            MonitoringSystems.Manager.UnitCreated -= OnMonitorUnitCreated;
            MonitoringSystems.Manager.UnitDisposed -= OnMonitorUnitDisposed;
            base.OnDestroy();
        }

        private void ManagerOnProfilingCompleted(IReadOnlyList<IMonitorUnit> staticUnits, IReadOnlyList<IMonitorUnit> instanceUnits)
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                return;
            }
#endif

            MonitoringSystems.Manager.UnitCreated += OnMonitorUnitCreated;
            MonitoringSystems.Manager.UnitDisposed += OnMonitorUnitDisposed;

            for (var i = 0; i < staticUnits.Count; i++)
            {
                OnMonitorUnitCreated(staticUnits[i]);
            }

            for (var i = 0; i < instanceUnits.Count; i++)
            {
                OnMonitorUnitCreated(instanceUnits[i]);
            }

            Visible = MonitoringSystems.Settings.OpenDisplayOnLoad;
        }

        /// <summary>
        /// The visible state of the UI.
        /// </summary>
        public abstract bool Visible { get; set; }

        /// <summary>
        /// Use to add UI elements for the passed unit.
        /// </summary>
        protected abstract void OnMonitorUnitCreated(IMonitorUnit unit);

        /// <summary>
        /// Use to remove UI elements for the passed unit.
        /// </summary>
        protected abstract void OnMonitorUnitDisposed(IMonitorUnit unit);
    }
}
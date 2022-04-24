using System.Collections.Generic;
using Baracuda.Monitoring.Interface;
using Baracuda.Monitoring.Management;
using UnityEngine;

namespace Baracuda.Monitoring.Display
{
    public abstract class MonitoringDisplay : MonitoredSingleton<MonitoringDisplay>
    {
        #region --- Static ---

        /// <summary>
        /// Set the active monitoring display visible.
        /// </summary>
        public static void ShowDisplay()
        {
            if (instance)
            {
                instance.Show();
            }
        }

        /// <summary>
        /// Hide the active monitoring display.
        /// </summary>
        public static void HideDisplay()
        {
            if (instance)
            {
                instance.Hide();
            }
        }

        /// <summary>
        /// Toggle the visibility of the active monitoring display.
        /// This method returns a value indicating the new visibility state.
        /// </summary>
        public static bool ToggleDisplay()
        {
            if (instance)
            {
                instance.Toggle();
            }

            return GetIsVisible();
        }

        /// <summary>
        /// Returns true if the there is an active monitoring display that is also visible.
        /// </summary>
        /// <returns></returns>
        public static bool GetIsVisible()
        {
            return instance != null && instance.IsVisible;
        }

        #endregion

        //--------------------------------------------------------------------------------------------------------------

        #region --- Setup ---

        private static MonitoringDisplay instance;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void InitializeMonitoringDisplay()
        {
            if (MonitoringSettings.GetInstance().EnableMonitoring)
            {
                MonitoringEvents.ProfilingCompleted += OnProfilingCompletedInternal;
            }
        }

        private static void OnProfilingCompletedInternal(IReadOnlyList<IMonitorUnit> staticUnits,
            IReadOnlyList<IMonitorUnit> instanceUnits)
        {
            var settings = MonitoringSettings.GetInstance();

            if (settings.DisplayDisplay == null)
            {
                return;
            }

            instance = Instantiate(settings.DisplayDisplay);

            MonitoringEvents.UnitCreated += instance.OnUnitCreated;
            MonitoringEvents.UnitDisposed += instance.OnUnitDisposed;

            for (var i = 0; i < staticUnits.Count; i++)
            {
                instance.OnUnitCreated(staticUnits[i]);
            }

            for (var i = 0; i < instanceUnits.Count; i++)
            {
                instance.OnUnitCreated(instanceUnits[i]);
            }

            if (settings.OpenDisplayOnLoad)
            {
                instance.Show();
            }
            else
            {
                instance.Hide();
            }
        }


        protected override void OnDestroy()
        {
            base.OnDestroy();
            MonitoringEvents.UnitCreated -= OnUnitCreated;
            MonitoringEvents.UnitDisposed -= OnUnitDisposed;
        }

        #endregion

        //--------------------------------------------------------------------------------------------------------------

        #region --- Abstract ---

        protected abstract bool IsVisible { get; }
        protected abstract void Show();
        protected abstract void Hide();
        protected abstract void Toggle();

        /*
         * Unit Handling   
         */

        protected abstract void OnUnitDisposed(IMonitorUnit unit);
        protected abstract void OnUnitCreated(IMonitorUnit unit);

        /*
         * Filtering   
         */

        protected abstract void ResetFilter();
        protected abstract void Filter(string filter);

        #endregion
    }
}
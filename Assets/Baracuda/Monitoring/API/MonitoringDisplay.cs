using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Baracuda.Monitoring.Interface;
using UnityEngine;

namespace Baracuda.Monitoring.API
{
    public static class MonitoringDisplay
    {
        #region --- API ---

        /// <summary>
        /// Set the active monitoring display visible.
        /// </summary>
        public static void ShowDisplay()
        {
            ShowDisplayInternal();
        }

        /// <summary>
        /// Hide the active monitoring display.
        /// </summary>
        public static void HideDisplay()
        {
            HideDisplayInternal();
        }

        /// <summary>
        /// Toggle the visibility of the active monitoring display.
        /// This method returns a value indicating the new visibility state.
        /// </summary>
        public static bool ToggleDisplay()
        {
            return ToggleDisplayInternal();
        }

        /// <summary>
        /// Returns true if the there is an active monitoring display that is also visible.
        /// </summary>
        /// <returns></returns>
        public static bool GetIsVisible()
        {
            return GetIsVisibleInternal();
        }

        #endregion

        //--------------------------------------------------------------------------------------------------------------

        #region --- Internal ---

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void ShowDisplayInternal()
        {
            if (controllerInstance)
            {
                controllerInstance.Show();
            }
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void HideDisplayInternal()
        {
            if (controllerInstance)
            {
                controllerInstance.Hide();
            }
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool ToggleDisplayInternal()
        {
            if (controllerInstance == null)
            {
                return false;
            }

            if (controllerInstance.IsVisible)
            {
                controllerInstance.Hide();
            }
            else
            {
                controllerInstance.Show();
            }

            return GetIsVisibleInternal();
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool GetIsVisibleInternal()
        {
            return controllerInstance != null && controllerInstance.IsVisible;
        }
        
        /*
         * Setup   
         */

        // singleton instance managed internally
        private static MonitoringDisplayController controllerInstance;

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

            if (settings.DisplayControllerDisplayController == null)
            {
                return;
            }
            
            if (!Application.isPlaying)
            {
                return;
            }
            
            controllerInstance = Object.Instantiate(settings.DisplayControllerDisplayController);
            
            Object.DontDestroyOnLoad(controllerInstance.gameObject);
            controllerInstance.gameObject.hideFlags = settings.ShowRuntimeUIController ? HideFlags.None : HideFlags.HideInHierarchy;

            MonitoringEvents.UnitCreated += controllerInstance.OnUnitCreated;
            MonitoringEvents.UnitDisposed += controllerInstance.OnUnitDisposed;

            for (var i = 0; i < staticUnits.Count; i++)
            {
                controllerInstance.OnUnitCreated(staticUnits[i]);
            }

            for (var i = 0; i < instanceUnits.Count; i++)
            {
                controllerInstance.OnUnitCreated(instanceUnits[i]);
            }

            if (settings.OpenDisplayOnLoad)
            {
                controllerInstance.Show();
            }
            else
            {
                controllerInstance.Hide();
            }
        }

        #endregion
    }
}

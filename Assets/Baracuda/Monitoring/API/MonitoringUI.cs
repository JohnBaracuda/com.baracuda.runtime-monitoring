// Copyright (c) 2022 Jonathan Lang

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Baracuda.Monitoring.Interface;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Baracuda.Monitoring.API
{
    public static class MonitoringUI
    {
        #region --- API ---

        /// <summary>
        /// Set the active monitoring display visible.
        /// </summary>
        public static void Show()
        {
#if !DISABLE_MONITORING
            ShowDisplayInternal();
#endif
        }

        /// <summary>
        /// Hide the active monitoring display.
        /// </summary>
        public static void Hide()
        {
#if !DISABLE_MONITORING
            HideDisplayInternal();
#endif
        }

        /// <summary>
        /// Toggle the visibility of the active monitoring display.
        /// This method returns a value indicating the new visibility state.
        /// </summary>
        public static bool ToggleDisplay()
        {
#if !DISABLE_MONITORING
            return ToggleDisplayInternal();
#else
            return false;
#endif
        }

        /// <summary>
        /// Returns true if the there is an active monitoring display that is also visible.
        /// </summary>
        /// <returns></returns>
        public static bool IsVisible()
        {
#if !DISABLE_MONITORING
            return GetIsVisibleInternal();
#else
            return false;
#endif
        }

        /// <summary>
        /// Get the current <see cref="MonitoringUIController"/>
        /// </summary>
        public static MonitoringUIController GetActiveUIController()
        {
#if !DISABLE_MONITORING
            return GetActiveUIControllerInternal();
#else
            return null;
#endif
        }

        /// <summary>
        /// Get the current <see cref="MonitoringUIController"/> as a concrete implementation of T.
        /// </summary>
        public static T GetActiveUIController<T>() where T : MonitoringUIController
        {
#if !DISABLE_MONITORING
            return GetActiveUIControllerInternal() as T;
#else
            return null;
#endif
        }

        /// <summary>
        /// Create a MonitoringUIController instance if there is none already. Disable 'Auto Instantiate UI' in the
        /// Monitoring Settings and use this method for more control over the timing in which the MonitoringUIController
        /// is instantiated.
        /// </summary>
        public static void CreateMonitoringUI()
        {
#if !DISABLE_MONITORING
            CreateMonitoringUIInternal();
#endif
        }

        /// <summary>
        /// Filter displayed units by their name, tags etc. 
        /// </summary>
        public static void Filter(string filter)
        {
#if !DISABLE_MONITORING
            FilterInternal(filter);
#endif
        }

        /// <summary>
        /// Reset active filter.
        /// </summary>
        public static void ResetFilter()
        {
#if !DISABLE_MONITORING
            ResetFilterInternal();
#endif
        }

        #endregion

        //--------------------------------------------------------------------------------------------------------------

#if !DISABLE_MONITORING
        #region --- Internal ---
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void ShowDisplayInternal()
        {
            if (controllerInstance)
            {
                controllerInstance.ShowMonitoringUI();
            }
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void HideDisplayInternal()
        {
            if (controllerInstance)
            {
                controllerInstance.HideMonitoringUI();
            }
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool ToggleDisplayInternal()
        {
            if (controllerInstance == null)
            {
                return false;
            }

            if (controllerInstance.IsVisible())
            {
                controllerInstance.HideMonitoringUI();
            }
            else
            {
                controllerInstance.ShowMonitoringUI();
            }

            return GetIsVisibleInternal();
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool GetIsVisibleInternal()
        {
            return controllerInstance != null && controllerInstance.IsVisible();
        }
        
        /*
         * Setup   
         */

        // singleton instance managed internally
        private static MonitoringUIController controllerInstance;
        private static bool bufferUICreation = false;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void InitializeMonitoringDisplay()
        {
            if (MonitoringSettings.GetInstance().EnableMonitoring)
            {
                MonitoringManager.ProfilingCompleted += OnProfilingCompletedInternal;
            }
        }

        private static void OnProfilingCompletedInternal(IReadOnlyList<IMonitorUnit> staticUnits,
            IReadOnlyList<IMonitorUnit> instanceUnits)
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                return;
            }
#endif
            var settings = MonitoringSettings.GetInstance();

            if (settings.AutoInstantiateUI || bufferUICreation)
            {
                InstantiateMonitoringUI(settings, staticUnits, instanceUnits);
            }
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void CreateMonitoringUIInternal()
        {
            bufferUICreation = true;

            if (!MonitoringManager.IsInitialized)
            {
                return;
            }
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                return;
            }
#endif
            
            // We return if there is an active UIController.
            if (GetActiveUIController())
            {
                Debug.Log("UIController already instantiated!");
                return;
            }

            var settings = MonitoringSettings.GetInstance();
            var instanceUnits = MonitoringManager.GetInstanceUnits();
            var staticUnits = MonitoringManager.GetStaticUnits();
            InstantiateMonitoringUI(settings, instanceUnits, staticUnits);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void InstantiateMonitoringUI(MonitoringSettings settings, IReadOnlyList<IMonitorUnit> staticUnits,
            IReadOnlyList<IMonitorUnit> instanceUnits)
        {
            if (settings.UIControllerUIController == null)
            {
                return;
            }
            
            controllerInstance = Object.Instantiate(settings.UIControllerUIController);
            
            Object.DontDestroyOnLoad(controllerInstance.gameObject);
            controllerInstance.gameObject.hideFlags = settings.ShowRuntimeUIController ? HideFlags.None : HideFlags.HideInHierarchy;

            MonitoringManager.UnitCreated += controllerInstance.OnUnitCreated;
            MonitoringManager.UnitDisposed += controllerInstance.OnUnitDisposed;
            
            Application.quitting += () =>
            {
                MonitoringManager.UnitCreated -= controllerInstance.OnUnitCreated;
                MonitoringManager.UnitDisposed -= controllerInstance.OnUnitDisposed;
            };
            
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
                controllerInstance.ShowMonitoringUI();
            }
            else
            {
                controllerInstance.HideMonitoringUI();
            }
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static MonitoringUIController GetActiveUIControllerInternal()
        {
            return controllerInstance;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void ResetFilterInternal()
        {
            foreach (var unit in MonitoringManager.GetAllMonitoringUnits())
            {
                unit.Enabled = true;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void FilterInternal(string filter)
        {
            var list = MonitoringManager.GetAllMonitoringUnits();
            for (var i = 0; i < list.Count; i++)
            {
                var unit = list[i];
                var tags = unit.Profile.Tags;
                var unitEnabled = false;
                if (unit.Name.IndexOf(filter, StringComparison.CurrentCultureIgnoreCase) >= 0)
                {
                    unit.Enabled = true;
                    continue;
                }
                for (var j = 0; j < tags.Length; j++)
                {
                    if (tags[j].IndexOf(filter, StringComparison.CurrentCultureIgnoreCase) >= 0)
                    {
                        unitEnabled = true;
                        break;
                    }
                }
                unit.Enabled = unitEnabled;
            }
        }

        #endregion
#endif //!DISABLE_MONITORING
    }
}

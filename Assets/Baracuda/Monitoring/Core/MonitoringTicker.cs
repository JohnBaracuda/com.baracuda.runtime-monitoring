// Copyright (c) 2022 Jonathan Lang

using System;
using System.Collections.Generic;
using Baracuda.Monitoring.API;
using Baracuda.Monitoring.Interface;
using UnityEngine;

namespace Baracuda.Monitoring.Core
{
    internal class MonitoringTicker : IMonitoringTicker
    {
        [Monitor] private static event Action UpdateTick;
        [Monitor] private static event Action ValidationTick;

        internal MonitoringTicker()
        {
            MonitoringManager.ProfilingCompleted += MonitoringEventsOnProfilingCompleted;
        }

        private void MonitoringEventsOnProfilingCompleted(IReadOnlyList<IMonitorUnit> staticUnits, IReadOnlyList<IMonitorUnit> instanceUnits)
        {
            if (!Application.isPlaying)
            {
                throw new Exception("Application must be in playmode!");
            }
            
            var sceneHook = new GameObject("Monitoring Scene Hook").AddComponent<SceneHook>();
            
            UnityEngine.Object.DontDestroyOnLoad(sceneHook);
            
            sceneHook.gameObject.hideFlags = MonitoringSettings.GetInstance().ShowRuntimeMonitoringObject 
                ? HideFlags.None 
                : HideFlags.HideInHierarchy;
            
            sceneHook.LateUpdateEvent += Tick;
        }

        private static float timer;
        
        private void Tick(float deltaTime)
        {
            //TODO: event based
            //TODO: refresh all units when visible again
            // if (!MonitoringUI.IsVisible())
            // {
            //     return;
            // }
            
            timer += deltaTime;
            if (timer < .05f)
            {
                return;
            }

            timer = 0;
            UpdateTick?.Invoke();
            if (MonitoringManager.ValidationTickEnabled)
            {
                ValidationTick?.Invoke();
            }
        }

        public void AddUpdateTicker(Action tickAction)
        {
            UpdateTick += tickAction;
        }

        public void RemoveUpdateTicker(Action tickAction)
        {
            UpdateTick -= tickAction;
        }

        public void AddValidationTicker(Action tickAction)
        {
            ValidationTick += tickAction;
        }

        public void RemoveValidationTicker(Action tickAction)
        {
            ValidationTick -= tickAction;
        }
    }
}

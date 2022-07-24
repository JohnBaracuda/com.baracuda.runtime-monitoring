// Copyright (c) 2022 Jonathan Lang

using System;
using System.Collections.Generic;
using Baracuda.Monitoring.API;
using Baracuda.Monitoring.Source.Interfaces;
using Baracuda.Monitoring.Source.Utilities;
using UnityEngine;

namespace Baracuda.Monitoring.Source.Systems
{
    internal class MonitoringTicker : IMonitoringTicker
    {
        /*
         * Properties   
         */
        
        public bool ValidationTickEnabled { get; set; } = true;

        //--------------------------------------------------------------------------------------------------------------
        
        [Monitor] private static event Action UpdateTick;
        [Monitor] private static event Action ValidationTick;

        //--------------------------------------------------------------------------------------------------------------
        
        internal MonitoringTicker()
        {
            MonitoringSystems.Resolve<IMonitoringManager>().ProfilingCompleted += MonitoringEventsOnProfilingCompleted;
        }

        private void MonitoringEventsOnProfilingCompleted(IReadOnlyList<IMonitorUnit> staticUnits, IReadOnlyList<IMonitorUnit> instanceUnits)
        {
            if (!Application.isPlaying)
            {
                throw new Exception("Application must be in playmode!");
            }
            
            var sceneHook = new GameObject("Monitoring Scene Hook").AddComponent<SceneHook>();
            
            UnityEngine.Object.DontDestroyOnLoad(sceneHook);
            
            sceneHook.gameObject.hideFlags = MonitoringSystems.Resolve<IMonitoringSettings>().ShowRuntimeMonitoringObject 
                ? HideFlags.None 
                : HideFlags.HideInHierarchy;
            
            sceneHook.LateUpdateEvent += Tick;
            
            MonitoringSystems.Resolve<IMonitoringUI>().VisibleStateChanged += visible =>
            {
                tickEnabled = visible;
                if (!visible)
                {
                    return;
                }

                UpdateTick?.Invoke();
                ValidationTick?.Invoke();
            };
        }

        private static float updateTimer;
        private static float validationTimer;
        private static bool tickEnabled;
        
        private void Tick(float deltaTime)
        {
            if (!tickEnabled)
            {
                return;
            }
            
            updateTimer += deltaTime;
            if (updateTimer > .05f)
            {
                updateTimer = 0;
                UpdateTick?.Invoke();
            }

            validationTimer += deltaTime;
            if (validationTimer > .1f)
            {
                validationTimer = 0;
                if (ValidationTickEnabled)
                {
                    ValidationTick?.Invoke();
                }
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

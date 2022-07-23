// Copyright (c) 2022 Jonathan Lang

using System;
using System.Collections.Generic;
using Baracuda.Monitoring.API;
using Baracuda.Monitoring.Interface;
using UnityEngine;

namespace Baracuda.Monitoring.Core
{
    internal static class MonitoringUpdate
    {
        private static readonly List<IMonitorUnit> updateUnits = new List<IMonitorUnit>(64);
        private static readonly List<IMonitorUnit> tickUnits  = new List<IMonitorUnit>(64);
        
        [Monitor]
        private static readonly List<IValidatable> validationUnits = new List<IValidatable>(32);

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Initialize()
        {
            MonitoringManager.ProfilingCompleted += MonitoringEventsOnProfilingCompleted;
        }

        private static void MonitoringEventsOnProfilingCompleted(IReadOnlyList<IMonitorUnit> staticUnits, IReadOnlyList<IMonitorUnit> instanceUnits)
        {
            if (!Application.isPlaying)
            {
                throw new Exception("Application must be in playmode!");
            }
            
            SetupUpdateHook();
            
            MonitoringManager.UnitCreated += MonitoringEventsOnUnitCreated;
            MonitoringManager.UnitDisposed  += MonitoringEventsOnUnitDisposed;
                
            for (var i = 0; i < staticUnits.Count; i++)
            {
                MonitoringEventsOnUnitCreated(staticUnits[i]);
            }
                
            for (var i = 0; i < instanceUnits.Count; i++)
            {
                MonitoringEventsOnUnitCreated(instanceUnits[i]);
            }
        }
        
        
        private static void MonitoringEventsOnUnitCreated(IMonitorUnit unit)
        {
            if (unit is IValidatable validatable && validatable.NeedsValidation)
            {
                validationUnits.Add(validatable);
            }
            
            if (unit.Profile.RequiresUpdate)
            {
                switch (unit.Profile.UpdateOptions)
                {
                    case UpdateOptions.FrameUpdate:
                        updateUnits.Add(unit);
                        break;
                    
                    case UpdateOptions.Auto: 
                    case UpdateOptions.TickUpdate:
                        tickUnits.Add(unit); 
                        break;
                }
            }
        }

        private static void MonitoringEventsOnUnitDisposed(IMonitorUnit unit)
        {
            if (unit is IValidatable validatable && validatable.NeedsValidation)
            {
                validationUnits.Remove(validatable);
            }
            
            if (unit.Profile.RequiresUpdate)
            {
                switch (unit.Profile.UpdateOptions)
                {
                    case UpdateOptions.FrameUpdate:
                        updateUnits.Remove(unit);
                        break;
                    
                    case UpdateOptions.Auto: 
                    case UpdateOptions.TickUpdate:
                        tickUnits.Remove(unit); 
                        break;
                }
            }
        }

        private static void SetupUpdateHook()
        {
            var ticker = new GameObject("Monitoring Ticker").AddComponent<MonitoringTicker>();
            ticker.OnLateUpdate += OnLateUpdate;
            ticker.OnTick += OnTick;
        }
        
        private static void OnLateUpdate()
        {
            for (var i = 0; i < updateUnits.Count; i++)
            {
                updateUnits[i].Refresh();
            }
        }

        private static void OnTick()
        {
            for (var i = 0; i < tickUnits.Count; i++)
            {
                tickUnits[i].Refresh();
            }

            if (MonitoringManager.AutoValidation)
            {
                for (var i = 0; i < validationUnits.Count; i++)
                {
                    validationUnits[i].Validate();
                }
            }
        }
    }
}

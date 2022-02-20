using System;
using System.Collections.Generic;
using Baracuda.Monitoring.Attributes;
using Baracuda.Monitoring.Interface;
using Baracuda.Monitoring.Management;
using UnityEngine;

namespace Baracuda.Monitoring.Internal
{
    internal static class MonitoringUpdate
    {
        private static readonly List<IMonitorUnit> _updateUnits = new List<IMonitorUnit>();
        private static readonly List<IMonitorUnit> _tickUnits  = new List<IMonitorUnit>();
        
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Initialize()
        {
            MonitoringEvents.ProfilingCompleted += MonitoringEventsOnProfilingCompleted;
        }

        private static void MonitoringEventsOnProfilingCompleted(IReadOnlyList<IMonitorUnit> staticUnits, IReadOnlyList<IMonitorUnit> instanceUnits)
        {
            if (!Application.isPlaying)
            {
                throw new Exception("Application must be in playmode!");
            }
            
            SetupUpdateHook();
            
            MonitoringEvents.UnitCreated += MonitoringEventsOnUnitCreated;
            MonitoringEvents.UnitDisposed  += MonitoringEventsOnUnitDisposed;
                
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
            if (unit.ExternalUpdateRequired)
            {
                switch (unit.Profile.Segment)
                {
                    case Segment.Update:
                        _updateUnits.Add(unit);
                        break;
                    
                    case Segment.Auto: 
                    case Segment.Tick:
                        _tickUnits.Add(unit); 
                        break;
                }
            }
        }

        private static void MonitoringEventsOnUnitDisposed(IMonitorUnit unit)
        {
            if (unit.ExternalUpdateRequired)
            {
                switch (unit.Profile.Segment)
                {
                    case Segment.Update:
                        _updateUnits.Remove(unit);
                        break;
                    
                    case Segment.Auto: 
                    case Segment.Tick:
                        _tickUnits.Remove(unit); 
                        break;
                }
            }
        }

        private static void SetupUpdateHook()
        {
            var hook = MonitoringUpdateHook.EnsureExist();
            hook.OnTick += OnTick;
            hook.OnUpdate += OnUpdate;
        }
        
        private static void OnUpdate()
        {
            for (var i = 0; i < _updateUnits.Count; i++)
            {
                _updateUnits[i].Refresh();
            }
        }

        private static void OnTick()
        {
            for (var i = 0; i < _tickUnits.Count; i++)
            {
                _tickUnits[i].Refresh();
            }
        }
    }
}

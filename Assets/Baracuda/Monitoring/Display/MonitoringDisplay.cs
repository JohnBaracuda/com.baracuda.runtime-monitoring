using System.Collections.Generic;
using Baracuda.Monitoring.Interface;
using Baracuda.Monitoring.Management;
using UnityEngine;

namespace Baracuda.Monitoring.Display
{
    public abstract class MonitoringDisplay : MonitoredSingleton<MonitoringDisplay>
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void InitializeMonitoringDisplay()
        {
            var settings = MonitoringSettings.Instance();

            if (!settings.EnableMonitoring)
            {
                return;
            }

            var displayHandlerInstance = Instantiate(settings.DisplayDisplay, Vector3.zero, Quaternion.identity);
            // MonitoringEvents.ProfilingCompleted += displayHandlerInstance.OnProfilingCompleted;
            // MonitoringEvents.ProfilingCompleted += delegate
            // {
            //     MonitoringEvents.UnitCreated += displayHandlerInstance.OnUnitCreated;
            //     MonitoringEvents.UnitDisposed += displayHandlerInstance.OnUnitDisposed;
            // };
        }
        
        public static void ShowDisplay(){}
        public static void HideDisplay(){}
        public static void ToggleDisplay(){}
        public static bool IsVisible() => false;

        // protected internal abstract void OnUnitDisposed(IMonitorUnit obj);
        //
        // protected internal abstract void OnUnitCreated(IMonitorUnit obj);
        //
        // protected internal abstract void OnProfilingCompleted(IReadOnlyList<IMonitorUnit> staticUnits,
        //     IReadOnlyList<IMonitorUnit> instanceUnits);
    }
}
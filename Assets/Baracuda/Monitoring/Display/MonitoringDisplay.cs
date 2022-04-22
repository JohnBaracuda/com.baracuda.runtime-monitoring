using Baracuda.Monitoring.Management;
using Baracuda.Threading;
using UnityEngine;

namespace Baracuda.Monitoring.Display
{
    public static class MonitoringDisplay
    {
        private static MonitoringDisplayHandler displayHandlerInstance;
        
        [RuntimeInitializeOnLoadMethod]
        private static async void InitializeMonitoringDisplay()
        {
            var settings = await Dispatcher.InvokeAsync(() => MonitoringSettings.Instance());

            if (!settings.EnableMonitoring)
            {
                return;
            }

            displayHandlerInstance = Object.Instantiate(settings.DisplayDisplayHandler, Vector3.zero, Quaternion.identity);
            // MonitoringEvents.ProfilingCompleted += displayHandlerInstance.OnProfilingCompleted;
            // MonitoringEvents.ProfilingCompleted += delegate
            // {
            //     MonitoringEvents.UnitCreated += displayHandlerInstance.OnUnitCreated;
            //     MonitoringEvents.UnitDisposed += displayHandlerInstance.OnUnitDisposed;
            // };
        }
    }
}

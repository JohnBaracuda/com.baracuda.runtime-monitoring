using Baracuda.Monitoring.API;
using UnityEngine;

namespace Baracuda.Monitoring.Core.Profiling
{
    internal static class SubsystemInstaller
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void InstallSubsystems()
        {
            //TODO: either port every other system to be interface based or make ticker also static
            MonitoringSystems.Register<IMonitoringTicker>(new MonitoringTicker());
            //Monitoring Manager
            //Monitoring UI
        }
    }
}
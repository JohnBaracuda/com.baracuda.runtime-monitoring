using Baracuda.Monitoring.API;

namespace Baracuda.Monitoring
{
    public static class MonitoringExtensions
    {
        public static void RegisterMonitor(this object target)
        {
            MonitoringUnitManager.RegisterTarget(target);
        }

        public static void UnregisterMonitor(this object target)
        {
            MonitoringUnitManager.UnregisterTarget(target);
        }
    }
}
namespace Baracuda.Monitoring.Management
{
    public static class MonitoringExtensions
    {
        public static void RegisterMonitor(this object target)
        {
            MonitoringManager.RegisterTarget(target);
        }

        public static void UnregisterMonitor(this object target)
        {
            MonitoringManager.UnregisterTarget(target);
        }
    }
}
// Copyright (c) 2022 Jonathan Lang (CC BY-NC-SA 4.0)
using Baracuda.Monitoring.API;

namespace Baracuda.Monitoring
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
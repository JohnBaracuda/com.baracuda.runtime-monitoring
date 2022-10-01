// Copyright (c) 2022 Jonathan Lang

using System.Runtime.CompilerServices;

namespace Baracuda.Monitoring
{
    public static class MonitoringExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void RegisterMonitor<T>(this T target) where T : class
        {
            MonitoringSystems.MonitoringManager.RegisterTarget(target);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void UnregisterMonitor<T>(this T target) where T : class
        {
            MonitoringSystems.MonitoringManager.UnregisterTarget(target);
        }
    }
}
// Copyright (c) 2022 Jonathan Lang

using System.Runtime.CompilerServices;

namespace Baracuda.Monitoring.Utilities
{
    /// <summary>
    /// Contains monitoring extensions
    /// </summary>
    public static class MonitoringExtensions
    {
        /// <summary>
        /// Register an object that is monitored during runtime.
        /// </summary>
        /// <param name="target"></param>
        /// <typeparam name="T"></typeparam>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void RegisterMonitor<T>(this T target) where T : class
        {
            MonitoringSystems.MonitoringManager.RegisterTarget(target);
        }

        /// <summary>
        /// Unregister an object that is monitored during runtime.
        /// </summary>
        /// <param name="target"></param>
        /// <typeparam name="T"></typeparam>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void UnregisterMonitor<T>(this T target) where T : class
        {
            MonitoringSystems.MonitoringManager.UnregisterTarget(target);
        }
    }
}
// Copyright (c) 2022 Jonathan Lang

using System;
using System.Runtime.CompilerServices;

namespace Baracuda.Monitoring
{
    /// <summary>
    ///     Contains monitoring extensions
    /// </summary>
    public static class MonitoringExtensions
    {
        /// <summary>
        ///     Register an object that is monitored during runtime.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void StartMonitoring<T>(this T target) where T : class
        {
            Monitor.StartMonitoring(target);
        }

        /// <summary>
        ///     Unregister an object that is monitored during runtime.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void StopMonitoring<T>(this T target) where T : class
        {
            Monitor.StopMonitoring(target);
        }


        #region Obsolete

        [Obsolete("Use StartMonitoring instead! This API will be removed in 4.0.0")]
        public static void RegisterMonitor<T>(this T target) where T : class
        {
            Monitor.StartMonitoring(target);
        }

        [Obsolete("Use StopMonitoring instead! This API will be removed in 4.0.0")]
        public static void UnregisterMonitor<T>(this T target) where T : class
        {
            Monitor.StopMonitoring(target);
        }

        #endregion
    }
}
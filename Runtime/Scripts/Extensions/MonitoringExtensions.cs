// Copyright (c) 2022 Jonathan Lang

using System;
using System.Runtime.CompilerServices;

namespace Baracuda.Monitoring
{
    /// <summary>
    /// Contains monitoring extensions
    /// </summary>
    public static class MonitoringExtensions
    {
        /// <summary>
        /// Register an object that is monitored during runtime.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void BeginMonitoring<T>(this T target) where T : class
        {
            Monitor.BeginMonitoring(target);
        }

        /// <summary>
        /// Unregister an object that is monitored during runtime.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void EndMonitoring<T>(this T target) where T : class
        {
            Monitor.EndMonitoring(target);
        }

        #region Obsolete

        [Obsolete("Use BeginMonitoring instead! This API will be removed in 4.0.0")]
        public static void RegisterMonitor<T>(this T target) where T : class
        {
            Monitor.BeginMonitoring(target);
        }

        [Obsolete("Use BeginMonitoring instead! This API will be removed in 4.0.0")]
        public static void UnregisterMonitor<T>(this T target) where T : class
        {
            Monitor.EndMonitoring(target);
        }

        #endregion
    }
}
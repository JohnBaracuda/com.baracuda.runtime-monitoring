// Copyright (c) 2022 Jonathan Lang

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace Baracuda.Monitoring
{
    /// <summary>
    ///     Class manages references for individual monitoring systems.
    /// </summary>
    public static class MonitoringSystems
    {
        // Persistent Cached Systems

        /// <summary>
        /// Core interface for accessing Runtime Monitoring functionality.
        /// </summary>
        public static IMonitoringManager MonitoringManager { get; private set; }

        /// <summary>
        /// Interface to access settings of for the monitoring system.
        /// </summary>
        public static IMonitoringSettings MonitoringSettings { get; private set; }

        /// <summary>
        /// Access to various monitoring utility methods.
        /// </summary>
        public static IMonitoringUtility MonitoringUtility { get; private set; }

        /// <summary>
        /// Access monitoring UI methods of the currently active UI instance.
        /// </summary>
        public static IMonitoringUI MonitoringUI { get; private set; }

        //--------------------------------------------------------------------------------------------------------------

        #region --- Registration ---

        private static readonly Dictionary<Type, object> systems = new Dictionary<Type, object>(8);

        /// <summary>
        /// Get the current system registered to the interface.
        /// </summary>
        /// <typeparam name="T">Type of the interface</typeparam>
        /// <returns></returns>
        /// <exception cref="SystemNotRegisteredException">Exception will occur if you are trying to access a system that is not registered.</exception>
        [Pure]
        [MethodImpl(MethodImplOptions.Synchronized)]
        public static T Resolve<T>() where T : class, IMonitoringSubsystem<T>
        {
            return systems.TryGetValue(typeof(T), out var system)
                ? (T) system
                : throw new SystemNotRegisteredException(typeof(T).Name);
        }

        /// <summary>
        /// Register a monitoring system. This API should only be called by the monitoring system itself.
        /// </summary>
        [SuppressMessage("ReSharper", "HeapView.PossibleBoxingAllocation")]
        public static T Register<T>(T system) where T : class, IMonitoringSubsystem<T>
        {
            var key = typeof(T);

            if (systems.ContainsKey(key))
            {
                systems[key] = system;
            }
            else
            {
                systems.Add(key, system);
            }

            switch (system)
            {
                case IMonitoringManager monitoringManager:
                    MonitoringManager = monitoringManager;
                    break;

                case IMonitoringSettings monitoringSettings:
                    MonitoringSettings = monitoringSettings;
                    break;

                case IMonitoringUtility monitoringUtility:
                    MonitoringUtility = monitoringUtility;
                    break;

                case IMonitoringUI monitoringUI:
                    MonitoringUI = monitoringUI;
                    break;
            }

            return system;
        }

        /// <summary>
        /// Exception will occur if you are trying to access a system that is not registered.
        /// </summary>
        public class SystemNotRegisteredException : Exception
        {
            internal SystemNotRegisteredException(string systemName) : base(
                $"System: [{systemName}] is not registered!")
            {
            }
        }

        #endregion
    }
}
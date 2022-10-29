// Copyright (c) 2022 Jonathan Lang

using Baracuda.Monitoring.Dummy;
using Baracuda.Monitoring.Systems;
using UnityEngine;

#pragma warning disable CS0067

namespace Baracuda.Monitoring
{
    /// <summary>
    /// Primary access to monitoring API and systems.
    /// </summary>
    public static class Monitor
    {
        /// <summary>
        /// Returns true once the system has been initialized.
        /// </summary>
        public static bool Initialized { get; private set; }

        /// <summary>
        /// Access to the monitoring settings asset. (Edit settings via: Tools > Runtime Monitoring)
        /// </summary>
        public static IMonitoringSettings Settings { get; }

        /// <summary>
        /// Access monitoring UI API.
        /// </summary>
        public static IMonitoringUI UI { get; }

        /// <summary>
        /// Access monitoring event handlers.
        /// </summary>
        public static IMonitoringEvents Events { get; }

        /// <summary>
        /// Primary interface to access cached data.
        /// </summary>
        public static IMonitoringRegistry Registry { get; }

        /// <summary>
        /// Register an object that is monitored.
        /// </summary>
        public static void StartMonitoring<T>(T target) where T : class
        {
            MonitoringRegistry.Singleton.RegisterTargetInternal(target);
        }

        /// <summary>
        /// Unregister an object that is monitored.
        /// </summary>
        public static void StopMonitoring<T>(T target) where T : class
        {
            MonitoringRegistry.Singleton.UnregisterTargetInternal(target);
        }

        internal static MonitoringLogger Logger { get; }
        internal static MonitoringTicker Ticker { get; }
        internal static ValidatorFactory ValidatorFactory { get; }
        internal static ValueProcessorFactory ProcessorFactory { get; }


        #region Installation

        static Monitor()
        {
            Settings = MonitoringSettings.Singleton;

            if (Settings.IsMonitoringEnabled)
            {
                Events = MonitoringEvents.Singleton;
                Ticker = new MonitoringTicker();
                Logger = new MonitoringLogger();
                ValidatorFactory = new ValidatorFactory();
                ProcessorFactory = new ValueProcessorFactory();
                UI = MonitoringDisplay.Singleton;
                Registry = MonitoringRegistry.Singleton;
            }
            else
            {
                var dummy = new MonitoringDummy();
                Registry = dummy;
                Events = dummy;
                UI = dummy;
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static async void InitializeProfiling()
        {
            if (Settings.IsMonitoringEnabled)
            {
                var profiler = new MonitoringProfiler();
                Initialized = await profiler.ProfileAsync();
            }
        }

        #endregion
    }
}

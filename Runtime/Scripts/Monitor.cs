// Copyright (c) 2022 Jonathan Lang

using Baracuda.Monitoring.Dummy;
using Baracuda.Monitoring.Systems;
using System.Collections.Generic;
using UnityEngine;

#pragma warning disable CS0067

namespace Baracuda.Monitoring
{
    #region Delegates

    /// <summary>
    /// Delegate for profiling completed listener
    /// </summary>
    public delegate void ProfilingCompletedListener(IReadOnlyList<IMonitorHandle> staticHandles,
        IReadOnlyList<IMonitorHandle> instanceHandles);

    #endregion

    /// <summary>
    /// Primary access to
    /// </summary>
    public static class Monitor
    {
        public static bool Initialized { get; private set; }
        public static IMonitoringSettings Settings { get; }
        public static IMonitoringUI UI { get; }
        public static IMonitoringEvents Events { get; }
        public static IMonitoringRegistry Registry { get; }

        /// <summary>
        /// Register an object that is monitored.
        /// </summary>
        public static void BeginMonitoring<T>(T target) where T : class
        {
            MonitoringRegistry.Singleton.RegisterTargetInternal(target);
        }

        /// <summary>
        /// Unregister an object that is monitored.
        /// </summary>
        public static void EndMonitoring<T>(T target) where T : class
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

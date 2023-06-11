// Copyright (c) 2022 Jonathan Lang

using Baracuda.Monitoring.Dummy;
using Baracuda.Monitoring.Systems;
using UnityEngine;

#pragma warning disable CS0067

namespace Baracuda.Monitoring
{
    /// <summary>
    ///     Primary access to monitoring API and systems.
    /// </summary>
#if UNITY_EDITOR
    [UnityEditor.InitializeOnLoadAttribute]
#endif
    public static class Monitor
    {
        #region API

        /// <summary>
        ///     Returns true once the system has been initialized.
        /// </summary>
        public static bool Initialized { get; private set; }

        /// <summary>
        ///     Access to the monitoring settings asset. (Edit settings via: Tools > Runtime Monitoring)
        /// </summary>
        public static IMonitoringSettings Settings { get; private set; }

        /// <summary>
        ///     Access monitoring UI API.
        /// </summary>
        public static IMonitoringUI UI { get; private set; }

        /// <summary>
        ///     Access monitoring event handlers.
        /// </summary>
        public static IMonitoringEvents Events { get; private set; }

        /// <summary>
        ///     Primary interface to access cached data.
        /// </summary>
        public static IMonitoringRegistry Registry { get; private set; }

        /// <summary>
        ///     Register an object that is monitored.
        /// </summary>
        public static void StartMonitoring<T>(T target) where T : class
        {
            InternalRegistry?.RegisterTargetInternal(target);
        }

        /// <summary>
        ///     Unregister an object that is monitored.
        /// </summary>
        public static void StopMonitoring<T>(T target) where T : class
        {
            InternalRegistry?.UnregisterTargetInternal(target);
        }

        #endregion


        #region Internal

        internal static MonitoringLogger Logger { get; private set; }
        internal static MonitoringUpdateEvents MonitoringUpdateEvents { get; private set; }
        internal static ValidatorFactory ValidatorFactory { get; private set; }
        internal static ValueProcessorFactory ProcessorFactory { get; private set; }

        internal static MonitoringRegistry InternalRegistry { get; private set; }
        internal static MonitoringDisplay InternalUI { get; private set; }
        internal static MonitoringEvents InternalEvents { get; private set; }

        private static volatile bool isConstructed;

        private const string SettingsNotFoundMessage =
            "[Runtime Monitoring] Could not locate settings! Please open Tools/Runtime Monitoring/Settings to create a new settings file!";

        #endregion


        #region Installation

        static Monitor()
        {
            Application.quitting += OnApplicationQuit;

#if UNITY_EDITOR
            UnityEditor.EditorApplication.delayCall += Construct;
#else
            Construct();
#endif
        }

        private static void Construct()
        {
            if (isConstructed)
            {
                return;
            }
            isConstructed = true;

            Settings = MonitoringSettings.Singleton;

            if (Settings != null && Settings.IsMonitoringEnabled)
            {
                InternalEvents = new MonitoringEvents();
                InternalRegistry = new MonitoringRegistry(InternalRegistry);
                InternalUI = new MonitoringDisplay();
                Events = InternalEvents;
                MonitoringUpdateEvents = new MonitoringUpdateEvents();
                Logger = new MonitoringLogger();
                ValidatorFactory = new ValidatorFactory();
                ProcessorFactory = new ValueProcessorFactory();
                UI = InternalUI;
                Registry = InternalRegistry;
            }
            else
            {
                var dummy = new MonitoringDummy();
                Registry = dummy;
                Events = dummy;
                UI = dummy;
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static async void Initialize()
        {
            Construct();

            if (Settings == null)
            {
                Debug.Log(SettingsNotFoundMessage);
                return;
            }

            if (Settings.IsMonitoringEnabled)
            {
                var profiler = new MonitoringProfiler();
                Initialized = await profiler.ProfileAsync();
            }
        }

        private static void OnApplicationQuit()
        {
            Initialized = false;
            isConstructed = false;
        }

        #endregion
    }
}
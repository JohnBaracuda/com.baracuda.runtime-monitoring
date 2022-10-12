// Copyright (c) 2022 Jonathan Lang

using Baracuda.Monitoring.Dummy;
using Baracuda.Monitoring.Interfaces;
using Baracuda.Monitoring.Systems;
using Baracuda.Threading;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Baracuda.Monitoring
{
    /// <summary>
    ///     Class manages references for individual monitoring systems.
    /// </summary>
#if UNITY_EDITOR
    [UnityEditor.InitializeOnLoad]
#endif
    public static class MonitoringSystems
    {
        // Persistent Cached Systems

        /// <summary>
        /// Core interface for accessing Runtime Monitoring functionality.
        /// </summary>
        public static IMonitoringManager Manager { get; private set; }

        /// <summary>
        /// Interface to access settings of for the monitoring system.
        /// </summary>
        public static IMonitoringSettings Settings { get; private set; }

        /// <summary>
        /// Access to various monitoring utility methods.
        /// </summary>
        public static IMonitoringUtility Utility { get; private set; }

        /// <summary>
        /// Access monitoring UI methods of the currently active UI instance.
        /// </summary>
        public static IMonitoringUI UI { get; private set; }


        internal static MonitoringUIManager InternalUI { get; private set; }
        internal static IMonitoringTicker Ticker { get; private set; }

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

        #region --- Installation ---

        private static bool installationCompleted;

#if UNITY_EDITOR
        static MonitoringSystems()
        {
            InstallMonitoringSystems();
        }
#endif

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void InitializeOnLoad()
        {
            InstallMonitoringSystems();
        }

        private static void InstallMonitoringSystems()
        {
            if (installationCompleted)
            {
                return;
            }
#if DISABLE_MONITORING
            InstallDummySystems();
#else
            var settings = MonitoringSettings.FindOrCreateSettingsAsset();
            if (settings.IsMonitoringEnabled)
            {
                InstallCoreSystems(settings);
            }
            else
            {
                InstallDummySystems();
            }
#endif
            installationCompleted = true;
        }

        private static void InstallDummySystems()
        {
            var dummy = new MonitoringSystemDummy();
            Register<IMonitoringSettings>(dummy);
            Register<IMonitoringManager>(dummy);
            Register<IMonitoringUI>(dummy);
            Register<IMonitoringUtility>(dummy);

            Manager = dummy;
            Settings = dummy;
            Utility = dummy;
            UI = dummy;
        }

        private static void InstallCoreSystems(IMonitoringSettings settings)
        {
            Settings = settings;
            Register(settings);

            var logger = new MonitoringLogging(settings);
            Register<IMonitoringLogger>(logger);

            var manager = new MonitoringManager();
            Manager = manager;
            Register<IMonitoringManager>(manager);
            Register<IMonitoringManagerInternal>(manager);

            var utility = new MonitoringUtility(manager);
            Utility = utility;
            Register<IMonitoringUtility>(utility);
            Register<IMonitoringUtilityInternal>(utility);


#if UNITY_EDITOR
            if (!UnityEditor.EditorApplication.isPlayingOrWillChangePlaymode)
            {
                return;
            }
#endif

            var ticker = new MonitoringTicker(manager);
            Ticker = ticker;
            Register<IMonitoringTicker>(ticker);

            var uiSystem = new MonitoringUIManager();
            UI = uiSystem;
            InternalUI = uiSystem;
            Register<IMonitoringUI>(uiSystem);

            Register<IValueProcessorFactory>(new ValueProcessorFactory(settings));
            Register<IValidatorFactory>(new ValidatorFactory());
            Register<IMonitoringProfiler>(new MonitoringProfiler(settings)).BeginProfiling(Dispatcher.RuntimeToken);
        }

        #endregion
    }
}
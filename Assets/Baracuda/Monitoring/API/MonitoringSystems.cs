// Copyright (c) 2022 Jonathan Lang

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using Baracuda.Monitoring.Source.Interfaces;
using Baracuda.Monitoring.Source.Systems;
using Baracuda.Threading;
using UnityEngine;

namespace Baracuda.Monitoring.API
{
    /// <summary>
    ///     Class manages references for individual monitoring systems.
    /// </summary>
    public static class MonitoringSystems
    {
        #region --- System Localization ---
        
        private static readonly Dictionary<Type, object> systems = new Dictionary<Type, object>(8);

        [Pure]
        [MethodImpl(MethodImplOptions.Synchronized)]
        public static T Resolve<T>() where T : class, IMonitoringSubsystem<T>
        {
            return systems.TryGetValue(typeof(T), out var system)
                ? (T) system
                : throw new SystemNotRegisteredException(typeof(T).Name);
        }

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

        private class SystemNotRegisteredException : Exception
        {
            public SystemNotRegisteredException(string systemName) : base(
                $"System: [{systemName}] is not registered!")
            {
            }
        }

        #endregion

        //--------------------------------------------------------------------------------------------------------------
        
        #region --- System Installation --- 
        
        /// <summary>
        /// Install core systems.
        /// </summary>
        static MonitoringSystems()
        {
            var manager = new Baracuda.Monitoring.Source.Systems.MonitoringManager();
            Register<IMonitoringManager>(manager);
            Register<IMonitoringManagerInternal>(manager);
            
            var utility = new MonitoringUtility(manager);
            Register<IMonitoringUtility>(utility);
            Register<IMonitoringUtilityInternal>(utility);
            
            var settings = MonitoringSettings.FindOrCreateSettingsAsset();
            Register<IMonitoringPlugin>(new MonitoringPluginData());
            Register<IMonitoringSettings>(settings);
            Register<IMonitoringLogger>(new MonitoringLogging(settings));
        }

        /// <summary>
        /// Install runtime systems. (Ticker etc.)
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void InstallSubsystems()
        {
            var manager = Resolve<IMonitoringManager>();
            var settings = Resolve<IMonitoringSettings>();
            var ticker = new MonitoringTicker(manager);

            Register<IMonitoringTicker>(ticker);
            Register<IMonitoringUI>(new MonitoringUISystem(manager, settings, ticker));
            Register<IValueProcessorFactory>(new ValueProcessorFactory(settings));
            Register<IValidatorFactory>(new ValidatorFactory());
        }

        /// <summary>
        /// Begin assembly profiling.
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        private static void InstallReflectionSubsystems()
        {
            Register<IMonitoringProfiler>(new MonitoringProfiler()).BeginProfiling(Dispatcher.RuntimeToken);
        }
        
        #endregion
    }
}
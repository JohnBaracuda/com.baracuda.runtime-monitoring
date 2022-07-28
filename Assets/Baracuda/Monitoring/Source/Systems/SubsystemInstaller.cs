// Copyright (c) 2022 Jonathan Lang

using Baracuda.Monitoring.API;
using Baracuda.Monitoring.Source.Interfaces;
using Baracuda.Threading;
using UnityEngine;

namespace Baracuda.Monitoring.Source.Systems
{
    /// <summary>
    /// Installs monitoring system dependencies.
    /// The order in which these systems are registered is important.
    /// </summary>
    internal static class SubsystemInstaller
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void InstallSubsystems()
        {
            var manager = new MonitoringManager();
            MonitoringSystems.Register<IMonitoringManager>(manager);
            MonitoringSystems.Register<IMonitoringManagerInternal>(manager);
            
            var utility = new MonitoringUtility(manager);
            MonitoringSystems.Register<IMonitoringUtility>(utility);
            MonitoringSystems.Register<IMonitoringUtilityInternal>(utility);

            var settings = MonitoringSettings.FindOrCreateSettingsAsset();
            var ticker = new MonitoringTicker(manager);
            MonitoringSystems.Register<IMonitoringSettings>(settings);
            MonitoringSystems.Register<IMonitoringTicker>(ticker);
            MonitoringSystems.Register<IMonitoringUI>(new MonitoringUISystem(manager, settings, ticker));
            MonitoringSystems.Register<IMonitoringPlugin>(new MonitoringPluginData());
            MonitoringSystems.Register<IMonitoringLogger>(new MonitoringLogging(settings));
            MonitoringSystems.Register<IValueProcessorFactory>(new ValueProcessorFactory(settings));
            MonitoringSystems.Register<IValidatorFactory>(new ValidatorFactory());
        }
        
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        private static void InstallReflectionSubsystems()
        {
            MonitoringSystems.Register<IMonitoringProfiler>(new MonitoringProfiler()).BeginProfiling(Dispatcher.RuntimeToken);
        }

#if UNITY_EDITOR
        [UnityEditor.InitializeOnLoadMethod]
        private static void InstallEditorSubsystems()
        {
            var settings = MonitoringSettings.FindOrCreateSettingsAsset();
            MonitoringSystems.Register<IMonitoringPlugin>(new MonitoringPluginData());
            MonitoringSystems.Register<IMonitoringSettings>(settings);
            MonitoringSystems.Register<IMonitoringLogger>(new MonitoringLogging(settings));
        }
#endif
    }
}
using Baracuda.Monitoring.API;
using Baracuda.Monitoring.Source.Interfaces;
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
            var monitoringManager = new MonitoringManagerSystem();
            MonitoringSystems.Register<IMonitoringManager>(monitoringManager);
            MonitoringSystems.Register<IMonitoringManagerInternal>(monitoringManager);
            
            MonitoringSystems.Register<IMonitoringSettings>(MonitoringSettings.FindOrCreateSettingsAsset());
            MonitoringSystems.Register<IMonitoringTicker>(new MonitoringTicker());
            MonitoringSystems.Register<IMonitoringUI>(new MonitoringUISystem());
            MonitoringSystems.Register<IMonitoringPlugin>(new MonitoringPluginData());
            MonitoringSystems.Register<IMonitoringLogger>(new MonitoringLogging());
            
            // Initialization systems will be flushed after profiling
            MonitoringSystems.Register<IValueProcessorFactory>(new ValueProcessorFactory());
            MonitoringSystems.Register<IValidatorFactory>(new ValidatorFactory());
        }
        
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        private static void InstallReflectionSubsystems()
        {
            MonitoringSystems.Register<IMonitoringProfiler>(new MonitoringProfiler());
        }

#if UNITY_EDITOR
        [UnityEditor.InitializeOnLoadMethod]
        private static void InstallEditorSubsystems()
        {
            MonitoringSystems.Register<IMonitoringPlugin>(new MonitoringPluginData());
            MonitoringSystems.Register<IMonitoringSettings>(MonitoringSettings.FindOrCreateSettingsAsset());
            MonitoringSystems.Register<IMonitoringLogger>(new MonitoringLogging());
        }
#endif
    }
}
using Baracuda.Monitoring.Interfaces;
using Baracuda.Threading;
using UnityEngine;

namespace Baracuda.Monitoring.Systems
{
#if UNITY_EDITOR
    [UnityEditor.InitializeOnLoad]
#endif
    public static class MonitoringInstaller
    {
#if UNITY_EDITOR
        static MonitoringInstaller()
        {
            InstallSystems();
        }
#endif

        /*
         * Install Systems
         */

#if !UNITY_EDITOR
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
#endif
        private static void InstallSystems()
        {
#if !DISABLE_MONITORING
            InstallCoreSystems();
#else
            InstallDummySystems();
#endif
        }

#if !DISABLE_MONITORING

        private static void InstallCoreSystems()
        {
            var manager = new MonitoringManager();
            MonitoringSystems.Register<IMonitoringManager>(manager);
            MonitoringSystems.Register<IMonitoringManagerInternal>(manager);

            var utility = new MonitoringUtility(manager);
            MonitoringSystems.Register<IMonitoringUtility>(utility);
            MonitoringSystems.Register<IMonitoringUtilityInternal>(utility);

            var settings = MonitoringSettings.FindOrCreateSettingsAsset();
            MonitoringSystems.Register<IMonitoringPlugin>(new MonitoringPluginData());
            MonitoringSystems.Register<IMonitoringSettings>(settings);
            MonitoringSystems.Register<IMonitoringLogger>(new MonitoringLogging(settings));

#if UNITY_EDITOR
            if (!UnityEditor.EditorApplication.isPlayingOrWillChangePlaymode)
            {
                return;
            }
#endif
            var ticker = new MonitoringTicker(manager);
            MonitoringSystems.Register<IMonitoringTicker>(ticker);
            MonitoringSystems.Register<IMonitoringUI>(new MonitoringUISystem(manager, settings, ticker));
            MonitoringSystems.Register<IValueProcessorFactory>(new ValueProcessorFactory(settings));
            MonitoringSystems.Register<IValidatorFactory>(new ValidatorFactory());
            MonitoringSystems.Register<IMonitoringProfiler>(new MonitoringProfiler()).BeginProfiling(Dispatcher.RuntimeToken);
        }

#else // DISABLE_MONITORING

        private static void InstallDummySystems()
        {
            MonitoringSystems.Register<IMonitoringPlugin>(new MonitoringPluginData());
            var settings = MonitoringSettings.FindOrCreateSettingsAsset();
            var dummy = new MonitoringSystemDummy();
            MonitoringSystems.Register<IMonitoringSettings>(settings);
            MonitoringSystems.Register<IMonitoringManager>(dummy);
            MonitoringSystems.Register<IMonitoringUI>(dummy);
            MonitoringSystems.Register<IMonitoringUtility>(dummy);

            MonitoringSystems.Register<IMonitoringManagerInternal>(dummy);
            MonitoringSystems.Register<IMonitoringUtilityInternal>(dummy);
            MonitoringSystems.Register<IMonitoringLogger>(dummy);
            MonitoringSystems.Register<IMonitoringTicker>(dummy);
            MonitoringSystems.Register<IValueProcessorFactory>(dummy);
            MonitoringSystems.Register<IValidatorFactory>(dummy);
            MonitoringSystems.Register<IMonitoringProfiler>(dummy);
        }
#endif
    }
}
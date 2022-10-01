using Baracuda.Monitoring.Core.Interfaces;
using Baracuda.Threading;
using UnityEngine;

namespace Baracuda.Monitoring.Core.Systems
{
#if UNITY_EDITOR
    [UnityEditor.InitializeOnLoad]
#endif
    internal static class MonitoringInstaller
    {

#if UNITY_EDITOR
        static MonitoringInstaller()
        {
            if (!UnityEditor.EditorApplication.isPlayingOrWillChangePlaymode)
            {
                InstallCoreSystems();
            }
        }
#endif

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void InitializeOnLoad()
        {
            InstallCoreSystems();
        }

        private static void InstallCoreSystems()
        {
            var manager = new MonitoringManager();
            MonitoringSystems.Register<IMonitoringManager>(manager);
            MonitoringSystems.Register<IMonitoringManagerInternal>(manager);

            var utility = new MonitoringUtility(manager);
            MonitoringSystems.Register<IMonitoringUtility>(utility);
            MonitoringSystems.Register<IMonitoringUtilityInternal>(utility);

            var settings = MonitoringSettings.FindOrCreateSettingsAsset();
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
            MonitoringSystems.Register<IMonitoringProfiler>(new MonitoringProfiler(settings)).BeginProfiling(Dispatcher.RuntimeToken);
        }
    }
}
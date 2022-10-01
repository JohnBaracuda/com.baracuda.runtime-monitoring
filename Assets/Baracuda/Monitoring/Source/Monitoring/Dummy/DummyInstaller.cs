namespace Baracuda.Monitoring.Dummy
{
#pragma warning disable
    internal static class DummyInstaller
    {
        private static void InstallDummySystems()
        {
            var dummy = new MonitoringSystemDummy();
            MonitoringSystems.Register<IMonitoringSettings>(dummy);
            MonitoringSystems.Register<IMonitoringManager>(dummy);
            MonitoringSystems.Register<IMonitoringUI>(dummy);
            MonitoringSystems.Register<IMonitoringUtility>(dummy);
        }
    }
}
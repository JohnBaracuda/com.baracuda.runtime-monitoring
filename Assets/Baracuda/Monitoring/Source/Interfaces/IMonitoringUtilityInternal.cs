namespace Baracuda.Monitoring.API
{
    internal interface IMonitoringUtilityInternal : IMonitoringSubsystem<IMonitoringUtilityInternal>
    {
        void AddFontHash(int fontHash);
    }
}
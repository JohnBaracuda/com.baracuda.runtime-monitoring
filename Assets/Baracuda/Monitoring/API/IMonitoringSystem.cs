namespace Baracuda.Monitoring.API
{
    /// <summary>
    /// Base interface for monitoring systems.
    /// </summary>
    /// <typeparam name="T">concrete interface for system</typeparam>
    public interface IMonitoringSystem<T> where T : class, IMonitoringSystem<T>
    {
    }
}
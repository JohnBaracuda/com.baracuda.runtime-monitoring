// Copyright (c) 2022 Jonathan Lang

namespace Baracuda.Monitoring.API
{
    /// <summary>
    /// Base interface for monitoring systems.
    /// </summary>
    /// <typeparam name="T">concrete interface for system</typeparam>
    public interface IMonitoringSubsystem<T> where T : class, IMonitoringSubsystem<T>
    {
    }
}
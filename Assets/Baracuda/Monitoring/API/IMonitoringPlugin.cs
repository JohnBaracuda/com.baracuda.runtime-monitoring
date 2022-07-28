// Copyright (c) 2022 Jonathan Lang

namespace Baracuda.Monitoring.API
{
    public interface IMonitoringPlugin : IMonitoringSubsystem<IMonitoringPlugin>
    {
        string Copyright { get; }
        string Version { get; }
        
        string Documentation { get; }
        string Repository { get; }
        string Website { get; }
    }
}
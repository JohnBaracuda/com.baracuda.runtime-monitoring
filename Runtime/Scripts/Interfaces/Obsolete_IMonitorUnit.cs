using System;

namespace Baracuda.Monitoring
{
    [Obsolete("Use IMonitorHandle instead! This API will be removed in 4.0.0")]
    public interface IMonitorUnit : IMonitorHandle
    {
    }
}
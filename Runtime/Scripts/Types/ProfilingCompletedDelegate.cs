using System.Collections.Generic;

namespace Baracuda.Monitoring
{
    /// <summary>
    /// Delegate for profiling completed listener
    /// </summary>
    public delegate void ProfilingCompletedDelegate(IReadOnlyList<IMonitorHandle> staticHandles, IReadOnlyList<IMonitorHandle> instanceHandles);
}
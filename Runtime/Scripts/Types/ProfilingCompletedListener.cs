using System.Collections.Generic;

namespace Baracuda.Monitoring
{
#pragma warning disable CS0067
    /// <summary>
    /// Delegate for profiling completed listener
    /// </summary>
    public delegate void ProfilingCompletedListener(IReadOnlyList<IMonitorHandle> staticHandles,
        IReadOnlyList<IMonitorHandle> instanceHandles);
}
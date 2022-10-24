using System;
using System.Collections.Generic;

namespace Baracuda.Monitoring
{
#pragma warning disable CS0067

    [Obsolete]
    public delegate void ProfilingCompletedListener(IReadOnlyList<IMonitorUnit> staticHandles,
        IReadOnlyList<IMonitorUnit> instanceHandles);

    /// <summary>
    /// Delegate for profiling completed listener
    /// </summary>
    public delegate void ProfilingCompletedDelegate(IReadOnlyList<IMonitorHandle> staticHandles,
        IReadOnlyList<IMonitorHandle> instanceHandles);
}
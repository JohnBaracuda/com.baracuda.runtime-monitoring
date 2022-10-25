using System;
using System.Collections.Generic;

namespace Baracuda.Monitoring
{
#pragma warning disable CS0067

    [Obsolete("Use ProfilingCompletedDelegate instead! This delegate will be removed in 4.0.0")]
    public delegate void ProfilingCompletedListener(IReadOnlyList<IMonitorUnit> staticHandles, IReadOnlyList<IMonitorUnit> instanceHandles);
}
// Copyright (c) 2022 Jonathan Lang

using System.Collections.Generic;

namespace Baracuda.Monitoring.API
{
    public delegate void ProfilingCompletedListener(IReadOnlyList<IMonitorUnit> staticUnits, IReadOnlyList<IMonitorUnit> instanceUnits);
}
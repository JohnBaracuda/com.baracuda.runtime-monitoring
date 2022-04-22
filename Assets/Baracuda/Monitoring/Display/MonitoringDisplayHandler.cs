using System.Collections.Generic;
using Baracuda.Monitoring.Interface;

namespace Baracuda.Monitoring.Display
{
    public abstract class MonitoringDisplayHandler : MonitoredSingleton<MonitoringDisplayHandler>
    {
        // protected internal abstract void OnUnitDisposed(IMonitorUnit obj);
        //
        // protected internal abstract void OnUnitCreated(IMonitorUnit obj);
        //
        // protected internal abstract void OnProfilingCompleted(IReadOnlyList<IMonitorUnit> staticUnits,
        //     IReadOnlyList<IMonitorUnit> instanceUnits);
    }
}
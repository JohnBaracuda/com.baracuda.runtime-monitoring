using System;
using Baracuda.Monitoring.Internal.Profiling;
using Baracuda.Monitoring.Internal.Units;

namespace Baracuda.Monitoring.Interface
{
    public interface IMonitorUnit
    {
        string Name { get; }
        
        /// <summary>
        /// Get the current value of the unit as a formatted string. 
        /// </summary>
        string GetValueFormatted { get; }

        /// <summary>
        /// Get the current value of the unit as a string.
        /// </summary>
        string GetValueRaw { get; }

        /// <summary>
        /// Determines if the unit must be updated/refreshed from an external source.
        /// </summary>
        bool ExternalUpdateRequired { get; }
        
        /// <summary>
        /// The target object of the unit. Null if static
        /// </summary>
        object Target { get; }

        /// <summary>
        /// The <see cref="MonitorProfile"/> of the unit.
        /// </summary>
        IMonitorProfile Profile { get; }

        /// <summary>
        /// Force the unit to update its state. This will invoke a <see cref="MonitorUnit.ValueUpdated"/> event.
        /// </summary>
        void Refresh();

        /// <summary>
        /// Event is invoked when the value of the unit has changed.
        /// </summary>
        event Action<string> ValueUpdated;

        /// <summary>
        /// Event is invoked when the unit is being disposed.
        /// </summary>
        event Action Disposing;
    }
}
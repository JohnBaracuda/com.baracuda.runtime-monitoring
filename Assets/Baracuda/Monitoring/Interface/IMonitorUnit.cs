// Copyright (c) 2022 Jonathan Lang (CC BY-NC-SA 4.0)
using System;
using Baracuda.Monitoring.Internal.Units;

namespace Baracuda.Monitoring.Interface
{
    public interface IMonitorUnit
    {
        string Name { get; }
        
        /// <summary>
        /// Get the current value or state of the monitored member as a formatted string. 
        /// </summary>
        string GetStateFormatted { get; }

        /// <summary>
        /// Get the current value or state of the monitored member as a string.
        /// </summary>
        string GetStateRaw { get; }

        /// <summary>
        /// Determines if the monitored member must be updated/refreshed from an external source.
        /// </summary>
        bool ExternalUpdateRequired { get; }
        
        /// <summary>
        /// The target object of the monitored member. Null if static
        /// </summary>
        object Target { get; }

        /// <summary>
        /// <see cref="IMonitorProfile"/> describing the monitored member. 
        /// </summary>
        IMonitorProfile Profile { get; }

        /// <summary>
        /// Force the monitored member to update its state. This will invoke a <see cref="MonitorUnit.ValueUpdated"/> event.
        /// </summary>
        void Refresh();

        /// <summary>
        /// Event is invoked when the value of the monitored member has changed.
        /// </summary>
        event Action<string> ValueUpdated;

        /// <summary>
        /// Event is invoked when the unit is being disposed.
        /// </summary>
        event Action Disposing;
    }
}
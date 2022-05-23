// Copyright (c) 2022 Jonathan Lang
using System;
using Baracuda.Monitoring.API;
using UnityEngine.Scripting;

namespace Baracuda.Monitoring
{
    /// <summary>
    /// Mark a Field, Property or Event to be monitored at runtime.
    /// When monitoring non static members, instances of the monitored class must be registered and unregistered
    /// when they are created and destroyed using:
    /// <see cref="MonitoringManager.RegisterTarget"/> or <see cref="MonitoringManager.UnregisterTarget"/>.
    /// This process can be simplified by using monitored base types:
    /// <br/><see cref="MonitoredObject"/>, <see cref="MonitoredBehaviour"/> or <see cref="MonitoredSingleton{T}"/>
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Event)]
    [Preserve]
    public class MonitorAttribute : Attribute
    {
        public UpdateOptions Update { get; set; } = UpdateOptions.Auto;

        public MonitorAttribute()
        {
        }
    }

    /// <summary>
    /// Set the update loop/segment during which a monitored members state should be evaluated.
    /// </summary>
    public enum UpdateOptions
    {
        /// <summary>
        /// If an update event is set, the state of the members will only be evaluated when the event is invoked. Else
        /// Tick is the preferred update interval. 
        /// </summary>
        Auto = 0,
        
        /// <summary>
        /// The members will not be evaluated except once on load. Use this option for constant values.
        /// </summary>
        DontUpdate = 1,
        
        /// <summary>
        /// The member will be evaluated on every LateUpdate.
        /// </summary>
        FrameUpdate = 2,
        
        /// <summary>
        /// The member will be evaluated on every Tick. Tick is a custom update cycle that is roughly called 30 times per second.
        /// </summary>
        TickUpdate = 4,
    }
}
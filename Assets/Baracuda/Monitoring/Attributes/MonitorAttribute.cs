using System;
using Baracuda.Monitoring.Management;
using JetBrains.Annotations;
using UnityEngine.Scripting;

namespace Baracuda.Monitoring.Attributes
{
    /// <summary>
    /// Mark a Field, Property or Event which will then be monitored during runtime. Use concretions of this attribute
    /// for more options depending on the targets category.
    /// <br/>Concretions are:
    /// <br/><see cref="MonitorValueAttribute"/>
    /// <br/><see cref="MonitorFieldAttribute"/>
    /// <br/><see cref="MonitorPropertyAttribute"/>
    /// <br/><see cref="MonitorEventAttribute"/>
    /// <br/> When monitoring non static members of a class, instances
    /// of the monitored class must be registered and unregistered when they are created and destroyed using:
    /// <see cref="MonitoringManager.RegisterTarget"/> or <see cref="MonitoringManager.UnregisterTarget"/>.
    /// This process can be simplified by using monitored base types for classes that you plan to monitor.
    /// <br/>These base types are:
    /// <br/><see cref="MonitoredObject"/>
    /// <br/><see cref="MonitoredBehaviour"/>
    /// <br/><see cref="MonitoredSingleton{T}"/>
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Event)]
    [MeansImplicitUse]
    [Preserve]
    public class MonitorAttribute : Attribute
    {
        public UpdateOptions Update { get; set; } = UpdateOptions.Auto;

        public MonitorAttribute()
        {
        }
    }

    /// <summary>
    /// Update segment during which a monitored members state should be evaluated.
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
        /// The member will be evaluated on every Update.
        /// </summary>
        FrameUpdate = 2,
        
        /// <summary>
        /// The member will be evaluated on every Tick. Tick is called in once every 50 milliseconds.
        /// </summary>
        TickUpdate = 4,
    }
}
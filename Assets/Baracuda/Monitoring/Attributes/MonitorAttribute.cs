using System;
using JetBrains.Annotations;

namespace Baracuda.Monitoring.Attributes
{
    /// <summary>
    /// Update segment during which a monitored members state should be evaluated.
    /// </summary>
    public enum Segment
    {
        /// <summary>
        /// If an update event is set, the state of the members will only be evaluated when the event is invoked. Else
        /// Tick is the preferred update interval. 
        /// </summary>
        Auto = 0,
        
        /// <summary>
        /// The members will not be evaluated except once on load.
        /// </summary>
        DontUpdate = 1,
        
        /// <summary>
        /// The member will be evaluated on every Update.
        /// </summary>
        Update = 2,
        
        /// <summary>
        /// The member will be evaluated on every Tick. Tick is called in once every 50 milliseconds.
        /// </summary>
        Tick = 4,
    }
    
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Event)]
    [MeansImplicitUse]
    public class MonitorAttribute : Attribute
    {
        public Segment Interval { get; set; } = Segment.Auto;

        public MonitorAttribute()
        {
        }
    }
}
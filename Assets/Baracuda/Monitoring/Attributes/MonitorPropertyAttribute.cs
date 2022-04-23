using System;
using Baracuda.Monitoring.Management;
using UnityEngine.Scripting;

namespace Baracuda.Monitoring.Attributes
{
    /// <summary>
    /// Mark a Property which will then be monitored during runtime.
    /// <br/> When monitoring non static members of a class, instances
    /// of the monitored class must be registered and unregistered when they are created and destroyed using:
    /// <see cref="MonitoringManager.RegisterTarget"/> or <see cref="MonitoringManager.UnregisterTarget"/>.
    /// This process can be simplified by using monitored base types for classes that you plan to monitor.
    /// These base types are:
    /// <br/><see cref="MonitoredObject"/>
    /// <br/><see cref="MonitoredBehaviour"/>
    /// <br/><see cref="MonitoredSingleton{T}"/>
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    [Preserve]
    public sealed class MonitorPropertyAttribute : MonitorValueAttribute
    {
        //TODO: is this really needed? 
        
        /// <summary>
        /// Target the backing field of the property directly.
        /// </summary>
        public bool TargetBacking { get; set; } = false;
        public bool GetBacking { get; set; } = false;
        public bool SetBacking { get; set; } = false;
        
        public MonitorPropertyAttribute()
        {
        }
    }
}
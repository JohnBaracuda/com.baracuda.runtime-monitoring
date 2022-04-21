using System;
using Baracuda.Monitoring.Management;
using UnityEngine.Scripting;

namespace Baracuda.Monitoring.Attributes
{
    /// <summary>
    /// Mark a Property or Field which will then be monitored during runtime.
    /// <br/> When monitoring non static members of a class, instances
    /// of the monitored class must be registered and unregistered when they are created and destroyed using:
    /// <see cref="MonitoringManager.RegisterTarget"/> or <see cref="MonitoringManager.UnregisterTarget"/>.
    /// This process can be simplified by using monitored base types for classes that you plan to monitor.
    /// These base types are:
    /// <br/><see cref="MonitoredObject"/>
    /// <br/><see cref="MonitoredBehaviour"/>
    /// <br/><see cref="MonitoredSingleton{T}"/>
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    [Preserve]
    public  class MonitorValueAttribute : MonitorAttribute
    {
        /// <summary>
        /// The name of an event that is invoked when the monitored value is updated. Use to reduce the evaluation of the
        /// monitored member. 
        /// </summary>
        public string Update { get; set; } = null;
    }
}
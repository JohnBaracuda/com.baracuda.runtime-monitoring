using System;
using Baracuda.Monitoring.Management;
using UnityEngine.Scripting;

namespace Baracuda.Monitoring.Attributes
{
    /// <summary>
    /// Mark an Event which will then be monitored during runtime.
    /// <br/> When monitoring non static members of a class, instances
    /// of the monitored class must be registered and unregistered when they are created and destroyed using:
    /// <see cref="MonitoringManager.RegisterTarget"/> or <see cref="MonitoringManager.UnregisterTarget"/>.
    /// This process can be simplified by using monitored base types for classes that you plan to monitor.
    /// These base types are:
    /// <br/><see cref="MonitoredObject"/>
    /// <br/><see cref="MonitoredBehaviour"/>
    /// <br/><see cref="MonitoredSingleton{T}"/>
    /// </summary>
    [AttributeUsage(AttributeTargets.Event)]
    [Preserve]
    public sealed class MonitorEventAttribute : MonitorAttribute
    {
        /// <summary>
        /// When enabled the signature of the event handler delegate is displayed. 
        /// </summary>
        public bool ShowSignature { get; set; } = true;
        
        /// <summary>
        /// When enabled the subscriber count of the event handler delegate is displayed.
        /// </summary>
        public bool ShowSubscriber { get; set; } = true;
        
        /// <summary>
        /// When enabled the actual subscriber count of the event handler is displayed including internal monitoring listener.
        /// </summary>
        public bool ShowTrueCount { get; set; } = false;
        
        public MonitorEventAttribute()
        {
        }
    }
}
// Copyright (c) 2022 Jonathan Lang
using System;
using Baracuda.Monitoring.API;
using UnityEngine.Scripting;

namespace Baracuda.Monitoring
{
    /// <summary>
    /// Mark a C# event to be monitored at runtime.
    /// When monitoring non static members, instances of the monitored class must be registered and unregistered
    /// when they are created and destroyed using:
    /// <see cref="MonitoringManager.RegisterTarget"/> or <see cref="MonitoringManager.UnregisterTarget"/>.
    /// This process can be simplified by using monitored base types:
    /// <br/><see cref="MonitoredObject"/>, <see cref="MonitoredBehaviour"/> or <see cref="MonitoredSingleton{T}"/>
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
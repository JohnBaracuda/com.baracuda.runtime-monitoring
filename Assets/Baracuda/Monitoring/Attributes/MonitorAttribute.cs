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
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Event | AttributeTargets.Method)]
    [Preserve]
    public class MonitorAttribute : Attribute
    {
        [Obsolete("Setting custom update intervals is no longer supported!")] 
        public UpdateOptions Update { get; set; } = default;
    }
}
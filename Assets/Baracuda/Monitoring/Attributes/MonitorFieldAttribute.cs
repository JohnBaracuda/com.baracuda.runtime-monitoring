using System;
using Baracuda.Monitoring.API;
using UnityEngine.Scripting;

namespace Baracuda.Monitoring
{
    /// <summary>
    /// Mark a Field to be monitored at runtime.
    /// When monitoring non static members, instances of the monitored class must be registered and unregistered
    /// when they are created and destroyed using:
    /// <see cref="MonitoringUnitManager.RegisterTarget"/> or <see cref="MonitoringUnitManager.UnregisterTarget"/>.
    /// This process can be simplified by using monitored base types:
    /// <br/><see cref="MonitoredObject"/>, <see cref="MonitoredBehaviour"/> or <see cref="MonitoredSingleton{T}"/>
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    [Preserve]
    public sealed class MonitorFieldAttribute : MonitorValueAttribute
    {
        public MonitorFieldAttribute()
        {
        }
    }
}
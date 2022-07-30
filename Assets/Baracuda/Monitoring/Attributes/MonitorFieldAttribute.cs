// Copyright (c) 2022 Jonathan Lang

using System;
using Baracuda.Monitoring.API;
using UnityEngine.Scripting;

namespace Baracuda.Monitoring
{
    /// <summary>
    /// Mark a Field to be monitored at runtime.
    /// When monitoring non static members, instances of the monitored class must be registered and unregistered.
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
// Copyright (c) 2022 Jonathan Lang

using System;
using UnityEngine.Scripting;

namespace Baracuda.Monitoring
{
    /// <summary>
    /// Mark a Property to be monitored at runtime.
    /// When monitoring non static members, instances of the monitored class must be registered and unregistered.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    [Preserve]
    public sealed class MonitorPropertyAttribute : MonitorValueAttribute
    {
        /// <summary>
        /// Mark a Property to be monitored at runtime.
        /// When monitoring non static members, instances of the monitored class must be registered and unregistered.
        /// </summary>
        public MonitorPropertyAttribute()
        {
        }
    }
}
// Copyright (c) 2022 Jonathan Lang

using System;

namespace Baracuda.Monitoring
{
    /// <summary>
    /// Mark a Field or Property to be monitored at runtime.
    /// When monitoring non static members, instances of the monitored class must be registered and unregistered
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class MonitorValueAttribute : MonitorAttribute
    {
        /// <summary>
        /// When enabled, the monitored value may be set by the MonitorUnit. This will enable UI scripts to set the value
        /// directly.
        /// </summary>
        public bool EnableSetAccess { get; set; }
    }
}
// Copyright (c) 2022 Jonathan Lang

using System;
using UnityEngine.Scripting;

namespace Baracuda.Monitoring
{
    /// <summary>
    /// Mark a Method to be monitored at runtime.
    /// When monitoring non static members, instances of the monitored class must be registered and unregistered
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    [Preserve]
    public class MonitorMethodAttribute : MonitorAttribute
    {
        /// <summary>
        /// Array contains values that will be used as a args for monitored methods.
        /// </summary>
        public object[] Args { get; }
        
        /// <summary>
        /// Mark a Method to be monitored at runtime.
        /// When monitoring non static members, instances of the monitored class must be registered and unregistered
        /// </summary>
        public MonitorMethodAttribute()
        {
        }
        
        /// <summary>
        /// Mark a Method to be monitored at runtime.
        /// When monitoring non static members, instances of the monitored class must be registered and unregistered
        /// </summary>
        public MonitorMethodAttribute(params object[] args)
        {
            Args = args;
        }
        
        /// <summary>
        /// Mark a Method to be monitored at runtime.
        /// When monitoring non static members, instances of the monitored class must be registered and unregistered
        /// </summary>
        public MonitorMethodAttribute(object arg1)
        {
            Args = new object[]{arg1};
        }
        
        /// <summary>
        /// Mark a Method to be monitored at runtime.
        /// When monitoring non static members, instances of the monitored class must be registered and unregistered
        /// </summary>
        public MonitorMethodAttribute(object arg1, object arg2)
        {
            Args = new object[]{arg1, arg2};
        }
        
        /// <summary>
        /// Mark a Method to be monitored at runtime.
        /// When monitoring non static members, instances of the monitored class must be registered and unregistered
        /// </summary>
        public MonitorMethodAttribute(object arg1, object arg2, object arg3)
        {
            Args = new object[]{arg1, arg2, arg3};
        }
    }
}
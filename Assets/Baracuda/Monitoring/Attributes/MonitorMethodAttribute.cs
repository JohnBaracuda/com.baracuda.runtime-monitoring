// Copyright (c) 2022 Jonathan Lang
 
using System;
using Baracuda.Monitoring.API;
using UnityEngine.Scripting;

namespace Baracuda.Monitoring
{
    /// <summary>
    /// Mark a Method to be monitored at runtime.
    /// When monitoring non static members, instances of the monitored class must be registered and unregistered
    /// when they are created and destroyed using:
    /// <see cref="MonitoringManager.RegisterTarget"/> or <see cref="MonitoringManager.UnregisterTarget"/>.
    /// This process can be simplified by using monitored base types:
    /// <br/><see cref="MonitoredObject"/>, <see cref="MonitoredBehaviour"/> or <see cref="MonitoredSingleton{T}"/>
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    [Preserve]
    public class MonitorMethodAttribute : MonitorAttribute
    {
        /// <summary>
        /// The name of an event that is invoked when the monitored value is updated. Use to reduce the evaluation of the
        /// monitored method. Events can be of type <see cref="Action"/> or <see cref="Action{T}"/>,
        /// with T being the type of the monitored value.
        /// </summary>
        /// <footer>Note: use the nameof keyword to pass the name of the event.</footer>
        public string UpdateEvent { get; set; } = null;
        
        /// <summary>
        /// Array contains values that will be used as a args for monitored methods.
        /// </summary>
        public object[] Args { get; }
        

        public MonitorMethodAttribute()
        {
        }
        
        public MonitorMethodAttribute(params object[] args)
        {
            Args = args;
        }
        
        public MonitorMethodAttribute(object arg1)
        {
            Args = new object[]{arg1};
        }
        public MonitorMethodAttribute(object arg1, object arg2)
        {
            Args = new object[]{arg1, arg2};
        }
        public MonitorMethodAttribute(object arg1, object arg2, object arg3)
        {
            Args = new object[]{arg1, arg2, arg3};
        }
    }
}
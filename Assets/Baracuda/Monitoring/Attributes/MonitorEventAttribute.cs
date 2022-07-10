// Copyright (c) 2022 Jonathan Lang
using System;
using Baracuda.Monitoring.API;
using Baracuda.Monitoring.Internal.Utilities;
using UnityEngine.Scripting;

namespace Baracuda.Monitoring
{
    [Flags]
    public enum EventDisplay
    {
        None = 0,
        SubCount = 1,
        InvokeCount = 2,
        TrueCount = 4,
        SubInfo = 8
    }
    
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
        /// When enabled, the subscriber count of the event handler delegate is displayed.
        /// </summary>
        public bool ShowSubscriberCount { get; set; } = true;

        /// <summary>
        /// When enabled, the amount the monitored event has been invoked will be displayed.
        /// </summary>
        public bool ShowInvokeCounter { get; set; } = true;
        
        /// <summary>
        /// When enabled, the actual subscriber count of the event handler is displayed including internal monitoring listener.
        /// </summary>
        public bool ShowTrueCount { get; set; } = true;

        /// <summary>
        /// When enabled, every subscribed delegate will be displayed. 
        /// </summary>
        public bool ShowSubscriberInfo { get; set; } = true;
        
        
        public MonitorEventAttribute()
        {
        }
        
        public MonitorEventAttribute(EventDisplay options)
        {
            ShowSubscriberCount = options.HasFlagUnsafe(EventDisplay.SubCount);
            ShowInvokeCounter = options.HasFlagUnsafe(EventDisplay.InvokeCount);
            ShowTrueCount = options.HasFlagUnsafe(EventDisplay.TrueCount);
            ShowSubscriberInfo = options.HasFlagUnsafe(EventDisplay.SubInfo);
        }

        //--------------------------------------------------------------------------------------------------------------

        #region --- Obsolete ---
        
        [Obsolete("Use ShowSubscriberCount instead!")]
        public bool ShowSubscriber { get; set; } = true;

        [Obsolete("Signature will always be displayed!")]
        public bool ShowSignature { get; set; } = true;
        
        #endregion
    }
}
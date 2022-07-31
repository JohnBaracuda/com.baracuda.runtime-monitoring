// Copyright (c) 2022 Jonathan Lang

using System;
using Baracuda.Monitoring.API;
using Baracuda.Monitoring.Source.Types;
using UnityEngine.Scripting;

namespace Baracuda.Monitoring
{
    /// <summary>
    /// Mark a C# event to be monitored at runtime.
    /// When monitoring non static members, instances of the monitored class must be registered and unregistered.
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

        /// <summary>
        /// When enabled, display the signature of the event.
        /// </summary>
        public bool ShowSignature { get; set; } = true;
        
        
        public MonitorEventAttribute()
        {
        }

        public MonitorEventAttribute(EventDisplay options)
        {
            var value = (int)options;
            ShowSubscriberCount = value.HasFlag32((int)EventDisplay.SubCount);
            ShowInvokeCounter = value.HasFlag32((int)EventDisplay.InvokeCount);
            ShowTrueCount = value.HasFlag32((int)EventDisplay.TrueCount);
            ShowSubscriberInfo = value.HasFlag32((int)EventDisplay.SubInfo);
            ShowSignature = value.HasFlag32((int) EventDisplay.Signature);
        }
    }
}
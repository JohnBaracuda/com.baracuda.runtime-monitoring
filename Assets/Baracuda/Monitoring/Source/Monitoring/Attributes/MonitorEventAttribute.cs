// Copyright (c) 2022 Jonathan Lang

using System;
using UnityEngine.Scripting;

namespace Baracuda.Monitoring
{
    /// <summary>
    /// Mark a C# event to be monitored at runtime.
    /// When monitoring non static members, instances of the monitored class must be registered and unregistered
    /// when they are created and destroyed using:
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

        /// <summary>
        /// When enabled, display the signature of the event.
        /// </summary>
        public bool ShowSignature { get; set; } = true;

        /// <summary>
        /// Mark a C# event to be monitored at runtime.
        /// When monitoring non static members, instances of the monitored class must be registered and unregistered
        /// when they are created and destroyed using:
        /// This process can be simplified by using monitored base types:
        /// <br/><see cref="MonitoredObject"/>, <see cref="MonitoredBehaviour"/> or <see cref="MonitoredSingleton{T}"/>
        /// </summary>
        public MonitorEventAttribute()
        {
        }

        /// <summary>
        /// Mark a C# event to be monitored at runtime.
        /// When monitoring non static members, instances of the monitored class must be registered and unregistered
        /// when they are created and destroyed using:
        /// This process can be simplified by using monitored base types:
        /// <br/><see cref="MonitoredObject"/>, <see cref="MonitoredBehaviour"/> or <see cref="MonitoredSingleton{T}"/>
        /// </summary>
        public MonitorEventAttribute(EventDisplay options)
        {
            ShowSubscriberCount = options.HasFlag(EventDisplay.SubCount);
            ShowInvokeCounter = options.HasFlag(EventDisplay.InvokeCount);
            ShowTrueCount = options.HasFlag(EventDisplay.TrueCount);
            ShowSubscriberInfo = options.HasFlag(EventDisplay.SubInfo);
            ShowSignature = options.HasFlag(EventDisplay.Signature);
        }
    }
}
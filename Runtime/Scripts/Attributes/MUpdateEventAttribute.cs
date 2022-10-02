// Copyright (c) 2022 Jonathan Lang

using System;
using UnityEngine.Scripting;

namespace Baracuda.Monitoring
{
    /// <summary>
    /// Pass the name of an event that is invoked when the monitored value is updated. Use to reduce the evaluation of the
    /// monitored member. Events can be of type <see cref="Action"/> or <see cref="Action{T}"/>, with T being the type of the monitored value.
    /// </summary>
    /// <footer>Note: use the nameof keyword to pass the name of the event.</footer>
    [Preserve]
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Method)]
    public class MUpdateEventAttribute : MonitoringMetaAttribute
    {
        /// <summary>
        /// The name of an event that is invoked when the monitored value is updated. Use to reduce the evaluation of the
        /// monitored member. Events can be of type <see cref="Action"/> or <see cref="Action{T}"/>, with T being the type of the monitored value.
        /// </summary>
        /// <footer>Note: use the nameof keyword to pass the name of the event.</footer>
        public string UpdateEvent { get; }

        /// <summary>
        /// Pass the name of an event that is invoked when the monitored value is updated. Use to reduce the evaluation of the
        /// monitored member. Events can be of type <see cref="Action"/> or <see cref="Action{T}"/>, with T being the type of the monitored value.
        /// </summary>
        /// <footer>Note: use the nameof keyword to pass the name of the event.</footer>
        public MUpdateEventAttribute(string updateEvent)
        {
            UpdateEvent = updateEvent;
        }
    }
}
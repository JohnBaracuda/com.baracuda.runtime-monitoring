// Copyright (c) 2022 Jonathan Lang

using System;
using UnityEngine.Scripting;

namespace Baracuda.Monitoring
{
    [Preserve]
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Method | AttributeTargets.Event)]
    public class MEnabledAttribute : MonitoringMetaAttribute
    {
        /// <summary>
        /// Set the default enabled state for the monitored member.
        /// </summary>
        public bool Enabled { get; }
        
        /// <summary>
        /// Set the default enabled state for the monitored member.
        /// </summary>
        public MEnabledAttribute(bool enabled)
        {
            Enabled = enabled;
        }
    }
}
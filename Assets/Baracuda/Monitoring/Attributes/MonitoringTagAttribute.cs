// Copyright (c) 2022 Jonathan Lang (CC BY-NC-SA 4.0)
using System;
using UnityEngine.Scripting;

namespace Baracuda.Monitoring
{
    /// <summary>
    /// Add tags to a monitored unit that can be used to provide additional meta data & filtering options for UI. 
    /// </summary>
    [Preserve]
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Event)]
    public sealed class MonitoringTagAttribute : MonitoringMetaAttribute
    {
        public string[] Tags { get; }

        public MonitoringTagAttribute(string tag)
        {
            Tags = new[] {tag};
        }
        
        public MonitoringTagAttribute(params string[] tags)
        {
            Tags = tags;
        }
    }
}
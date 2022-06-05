// Copyright (c) 2022 Jonathan Lang
using System;
using UnityEngine.Scripting;

namespace Baracuda.Monitoring
{
    /// <summary>
    /// Add tags to a monitored unit that can be used to provide additional meta data & filtering options for UI. 
    /// </summary>
    [Preserve]
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Event)]
    public class MTagAttribute : MonitoringMetaAttribute
    {
        public string[] Tags { get; }

        public MTagAttribute(string tag)
        {
            Tags = new[] {tag};
        }
        
        public MTagAttribute(string tag1, string tag2)
        {
            Tags = new[] {tag1, tag2};
        }
        
        public MTagAttribute(string tag1, string tag2, string tag3)
        {
            Tags = new[] {tag1, tag2, tag3};
        }
        
        public MTagAttribute(params string[] tags)
        {
            Tags = tags;
        }
    }
}
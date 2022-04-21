using System;
using UnityEngine.Scripting;

namespace Baracuda.Monitoring.Attributes
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Event)]
    [Preserve]
    public sealed class TagAttribute : MonitoringMetaAttribute
    {
        public string[] Tags { get; }

        public TagAttribute(string tag)
        {
            Tags = new[] {tag};
        }
        
        public TagAttribute(params string[] tags)
        {
            Tags = tags;
        }
    }
}
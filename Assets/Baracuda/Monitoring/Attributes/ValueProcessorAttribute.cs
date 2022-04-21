using System;
using UnityEngine.Scripting;

namespace Baracuda.Monitoring.Attributes
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    [Preserve]
    public sealed class ValueProcessorAttribute : Attribute
    {
        public string Processor { get; }
        
        public ValueProcessorAttribute(string processor)
        {
            Processor = processor;
        }
    }
}
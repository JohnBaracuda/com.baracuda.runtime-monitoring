using System;

namespace Baracuda.Monitoring.Attributes
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public sealed class ValueProcessorAttribute : Attribute
    {
        public readonly string Processor;
        public ValueProcessorAttribute(string processor)
        {
            Processor = processor;
        }
    }
}
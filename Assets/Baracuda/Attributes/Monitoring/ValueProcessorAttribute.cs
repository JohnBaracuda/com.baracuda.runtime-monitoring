using System;

namespace Baracuda.Attributes.Monitoring
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
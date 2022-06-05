using System;

namespace Baracuda.Monitoring
{
    [Obsolete("use MValueProcessorAttribute instead!")]
    public sealed class ValueProcessorAttribute : MValueProcessorAttribute
    {
        public ValueProcessorAttribute(string processorMethod) : base(processorMethod)
        {
        }
    }
}
using System;

namespace Baracuda.Attributes.Monitoring
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public  class MonitorValueAttribute : MonitorAttribute
    {
        public string Update { get; set; } = null;
    }
}
using System;

namespace Baracuda.Monitoring.Attributes
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public  class MonitorValueAttribute : MonitorAttribute
    {
        public string Update { get; set; } = null;
    }
}
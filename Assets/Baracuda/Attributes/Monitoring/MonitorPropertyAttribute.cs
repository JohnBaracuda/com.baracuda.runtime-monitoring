using System;

namespace Baracuda.Attributes.Monitoring
{
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class MonitorPropertyAttribute : MonitorValueAttribute
    {
        public bool TargetBacking { get; set; } = false;
        public bool GetBacking { get; set; } = false;
        public bool SetBacking { get; set; } = false;
        
        public MonitorPropertyAttribute()
        {
        }
    }
}
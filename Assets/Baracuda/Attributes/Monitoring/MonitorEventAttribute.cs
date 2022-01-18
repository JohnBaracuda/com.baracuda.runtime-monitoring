using System;

namespace Baracuda.Attributes.Monitoring
{
    [AttributeUsage(AttributeTargets.Event)]
    public sealed class MonitorEventAttribute : MonitorAttribute
    {
        public bool Refresh { get; set; } = true;
        public bool ShowSignature { get; set; } = true;
        public bool ShowSubscriber { get; set; } = true;
        public bool ShowTrueCount { get; set; } = false;
        
        public MonitorEventAttribute()
        {
        }
    }
}
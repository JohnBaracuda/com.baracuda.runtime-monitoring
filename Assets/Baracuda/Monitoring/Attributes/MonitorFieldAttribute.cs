using System;

namespace Baracuda.Monitoring.Attributes
{
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class MonitorFieldAttribute : MonitorValueAttribute
    {
        public MonitorFieldAttribute()
        {
        }
    }
}
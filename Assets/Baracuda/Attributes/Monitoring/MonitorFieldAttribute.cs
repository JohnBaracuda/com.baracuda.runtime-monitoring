using System;

namespace Baracuda.Attributes.Monitoring
{
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class MonitorFieldAttribute : MonitorValueAttribute
    {
        public MonitorFieldAttribute()
        {
        }
    }
}
using System;

namespace Baracuda.Monitoring.Attributes
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public abstract class MonitorAsAttribute : Attribute
    {
    }
    
    public sealed class MonitorAsProgressBarAttribute : MonitorAsAttribute
    {
        public int Segments { get; set; }
        public float MinValue { get; set; }
        public float MaxValue { get; set; }

        public MonitorAsProgressBarAttribute(float min, float max)
        {
            MinValue = min;
            MaxValue = max;
        }
    }
}
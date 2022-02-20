using System;

namespace Baracuda.Monitoring.Attributes
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public sealed class ProgressBarAttribute : Attribute
    {
        public int Segments { get; set; }
        public float MinValue { get; set; }
        public float MaxValue { get; set; }

        public ProgressBarAttribute(float min, float max, int segments = 0)
        {
            MinValue = min;
            MaxValue = max;
            Segments = segments;
        }
    }
}
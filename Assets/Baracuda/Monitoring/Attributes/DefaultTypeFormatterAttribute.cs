using System;
using JetBrains.Annotations;

namespace Baracuda.Monitoring.Attributes
{
    [AttributeUsage(AttributeTargets.Field)]
    [MeansImplicitUse]
    public sealed class DefaultTypeFormatterAttribute : Attribute
    {
        public readonly Type Type;

        public DefaultTypeFormatterAttribute(Type type)
        {
            Type = type;
        }
    }
}
using System;
using JetBrains.Annotations;

namespace Baracuda.Attributes.Monitoring
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
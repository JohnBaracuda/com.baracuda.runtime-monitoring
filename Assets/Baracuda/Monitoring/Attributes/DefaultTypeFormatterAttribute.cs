using System;
using JetBrains.Annotations;
using UnityEngine.Scripting;

namespace Baracuda.Monitoring.Attributes
{
    [AttributeUsage(AttributeTargets.Field)]
    [MeansImplicitUse]
    [Preserve]
    public sealed class DefaultTypeFormatterAttribute : Attribute
    {
        public readonly Type Type;

        public DefaultTypeFormatterAttribute(Type type)
        {
            Type = type;
        }
    }
}
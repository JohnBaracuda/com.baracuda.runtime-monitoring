using System;
using UnityEngine.Scripting;

namespace Baracuda.Monitoring
{
    /// <summary>
    /// Inherit from this attribute to use it as a proxy to create custom attributes that wrap multiple monitoring attributes.
    /// </summary>
    [Preserve]
    [AttributeUsage(MonitoringMetaAttribute.Targets)]
    public abstract class MAttributeCollection : Attribute
    {
    }
}
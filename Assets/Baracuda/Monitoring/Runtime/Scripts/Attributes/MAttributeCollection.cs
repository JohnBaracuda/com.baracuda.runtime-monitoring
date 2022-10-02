using System;
using UnityEngine.Scripting;

namespace Baracuda.Monitoring
{
    /// <summary>
    /// Inherit from this attribute to use it as a proxy to create custom attributes that wrap multiple monitoring attributes.
    /// </summary>
    [Preserve]
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Event | AttributeTargets.Method | AttributeTargets.Class)]
    public abstract class MAttributeCollection : Attribute
    {
    }
}
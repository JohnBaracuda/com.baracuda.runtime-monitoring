using System;
using UnityEngine.Scripting;

namespace Baracuda.Monitoring
{
    [Preserve]
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Event | AttributeTargets.Method | AttributeTargets.Class)]
    public abstract class MAttributeCollection : Attribute
    {
        
    }
}
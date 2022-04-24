using System;
using Baracuda.Monitoring.Attributes;
using UnityEngine.Scripting;

namespace Baracuda.Monitoring.UI.UIToolkit
{
    /// <summary>
    /// Meta attribute can be used to provide additional USS style classes that are applied for a monitor unit when using
    /// Unities new UI system.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Event)]
    [Preserve]
    public sealed class StyleAttribute : MonitoringMetaAttribute
    {
        public string[] ClassList { get; }
        
        public StyleAttribute(params string[] classList)
        {
            ClassList = classList;
        }
        
        public StyleAttribute(string @class)
        {
            ClassList = new []{@class};
        }
    }
}
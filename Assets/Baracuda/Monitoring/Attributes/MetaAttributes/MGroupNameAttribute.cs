using System;
using UnityEngine.Scripting;

namespace Baracuda.Monitoring
{
    [Preserve]
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Event | AttributeTargets.Method | AttributeTargets.Class)]
    public class MGroupNameAttribute : MonitoringMetaAttribute
    {
        /// <summary>
        /// Manually set the group for the element.
        /// </summary>
        public readonly string GroupName;

        /// <summary>
        /// Manually set the group for the element.
        /// </summary>
        public MGroupNameAttribute(string groupName)
        {
            GroupName = groupName;
        }
    }
}
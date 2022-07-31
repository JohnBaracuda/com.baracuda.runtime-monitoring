using System;

namespace Baracuda.Monitoring
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Method | AttributeTargets.Event)]
    public class MEnabledAttribute : MonitoringMetaAttribute
    {
        /// <summary>
        /// Set the default enabled state for the monitored member.
        /// </summary>
        public bool Enabled { get; }
        
        /// <summary>
        /// Set the default enabled state for the monitored member.
        /// </summary>
        public MEnabledAttribute(bool enabled)
        {
            Enabled = enabled;
        }
    }
}
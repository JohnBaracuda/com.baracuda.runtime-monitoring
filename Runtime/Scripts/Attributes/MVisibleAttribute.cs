using System;

namespace Baracuda.Monitoring
{
    /// <summary>
    /// Set the default visible state for the monitored member.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Method | AttributeTargets.Event | AttributeTargets.Class)]
    public class MVisibleAttribute : MonitoringMetaAttribute
    {
        /// <summary>
        /// Set the default visible state for the monitored member.
        /// </summary>
        public bool Visible { get; }

        /// <summary>
        /// Set the default visible state for the monitored member.
        /// </summary>
        public MVisibleAttribute(bool visible)
        {
            Visible = visible;
        }
    }
}
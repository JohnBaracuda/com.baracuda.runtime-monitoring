using System;
namespace Baracuda.Monitoring.Attributes
{
    /// <summary>
    /// Disable monitoring for the target assembly or class.
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Class | AttributeTargets.Struct)]
    public class DisableMonitoringAttribute : Attribute
    {
    }
}

using System;
using Baracuda.Attributes.Monitoring;

[assembly: DisableMonitoring]
namespace Baracuda.Attributes.Monitoring
{
    /// <summary>
    /// Disable monitoring for the target assembly or class.
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Class | AttributeTargets.Struct)]
    public class DisableMonitoringAttribute : Attribute
    {
    }
}

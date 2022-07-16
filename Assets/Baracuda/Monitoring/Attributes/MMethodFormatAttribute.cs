using System;
using UnityEngine.Scripting;

namespace Baracuda.Monitoring
{
    [Preserve]
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Parameter)]
    public class MMethodFormatAttribute : MonitoringMetaAttribute
    {
        
    }
}
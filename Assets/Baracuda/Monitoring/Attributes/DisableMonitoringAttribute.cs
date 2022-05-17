// Copyright (c) 2022 Jonathan Lang (CC BY-NC-SA 4.0)
using System;
using UnityEngine.Scripting;

namespace Baracuda.Monitoring
{
    /// <summary>
    /// Disable monitoring for the target assembly or class.
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Class | AttributeTargets.Struct)]
    [Preserve]
    public class DisableMonitoringAttribute : Attribute
    {
    }
}

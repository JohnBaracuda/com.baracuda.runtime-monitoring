// Copyright (c) 2022 Jonathan Lang
using System;
using Baracuda.Monitoring.Interface;
using UnityEngine.Scripting;

namespace Baracuda.Monitoring
{
    /// <summary>
    /// Attributes inheriting from this class can be used to provide monitoring units with additional meta data.
    /// These attributes will be cached on the Monitoring Profile and can be queried with <see cref="IMonitorProfile.TryGetMetaAttribute{TAttribute}"/>.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Event)]
    [Preserve]
    public abstract class MonitoringMetaAttribute : Attribute
    {
    }
}
using System;
using System.Reflection;
using Baracuda.Monitoring.Attributes;
using Baracuda.Monitoring.Internal.Utilities;

namespace Baracuda.Monitoring.Interface
{
    /// <summary>
    /// Interface provides access to data describing the profile of a monitored member. 
    /// </summary>
    public interface IMonitorProfile
    {
        /// <summary>
        /// Instance of the attribute that is used to flag the target member to be monitored.
        /// </summary>
        MonitorAttribute Attribute { get; }
        MemberInfo MemberInfo { get; }
        UnitType UnitType { get; }
        UpdateOptions UpdateOptions { get; }
        Type UnitTargetType { get; }
        Type UnitValueType { get; }
        bool IsStatic { get; }
        FormatData FormatData { get; }
        
        bool TryGetMetaAttribute<TAttribute>(out TAttribute attribute) where TAttribute : MonitoringMetaAttribute;
        TAttribute GetMetaAttribute<TAttribute>() where TAttribute : MonitoringMetaAttribute;
    }
}
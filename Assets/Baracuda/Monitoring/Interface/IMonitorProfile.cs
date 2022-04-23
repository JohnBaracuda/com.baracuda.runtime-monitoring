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
        Type UnitDeclaringType { get; }
        bool IsStatic { get; }
        
        // TODO: outsource from profile
        
        string[] Tags { get; }
        string Label { get; }
        string Format { get; }
        int FontSize { get; }
        UIPosition Position { get; }
        bool AllowGrouping { get; }
        string GroupName { get; }
        bool TryGetMetaAttribute<TAttribute>(out TAttribute attribute) where TAttribute : MonitoringMetaAttribute;
    }
}
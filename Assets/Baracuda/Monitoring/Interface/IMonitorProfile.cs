// Copyright (c) 2022 Jonathan Lang
using System;
using System.Reflection;
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
        
        /// <summary>
        /// Member info of the monitored member.
        /// </summary>
        MemberInfo MemberInfo { get; }
        
        /// <summary>
        /// The type of the member (either field, property or event)
        /// </summary>
        UnitType UnitType { get; }
        
        /// <summary>
        /// Enum determines how often and if a monitored member is evaluated.
        /// </summary>
        UpdateOptions UpdateOptions { get; }
        
        bool RequiresUpdate { get; }
        
        /// <summary>
        /// The target or declaring type of the monitored member.
        /// </summary>
        Type UnitTargetType { get; }
        
        /// <summary>
        /// The (value)type of the monitored member.
        /// </summary>
        Type UnitValueType { get; }
        
        /// <summary>
        /// Indicates if monitored member is static or not.
        /// </summary>
        bool IsStatic { get; }
        
        /// <summary>
        /// Object contains information about label, fontsize and more ui / display related data. 
        /// </summary>
        FormatData FormatData { get; }
        
        /// <summary>
        /// String array that contains additional meta data & filtering options for UI.
        /// </summary>
        string[] Tags { get; }
        
        /// <summary>
        /// Try to get a MonitoringMetaAttribute.
        /// </summary>
        bool TryGetMetaAttribute<TAttribute>(out TAttribute attribute) where TAttribute : MonitoringMetaAttribute;
        
        /// <summary>
        /// The monitoring profiler caches every additional attribute that inherits from MonitoringMetaAttribute on
        /// the profile. You can access these custom attributes during runtime using this method without the need of
        /// reflection code.
        /// </summary>
        TAttribute GetMetaAttribute<TAttribute>() where TAttribute : MonitoringMetaAttribute;
    }
}
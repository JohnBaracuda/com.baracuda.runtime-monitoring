// Copyright (c) 2022 Jonathan Lang

using System;
using System.Reflection;

namespace Baracuda.Monitoring
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
        MemberType MemberType { get; }

        /// <summary>
        /// True if the unit receives a custom tick event.
        /// </summary>
        bool ReceiveTick { get; }

        /// <summary>
        /// Get the default enabled state of the unit.
        /// </summary>
        bool DefaultEnabled { get; }

        /// <summary>
        /// The target or declaring type of the monitored member.
        /// </summary>
        Type DeclaringType { get; }

        /// <summary>
        /// The type of the monitored member.
        /// </summary>
        Type MonitoredMemberType { get; }

        /// <summary>
        /// Indicates if monitored member is static or not.
        /// </summary>
        bool IsStatic { get; }

        /// <summary>
        /// Object contains information about label, fontsize and more ui / display related data.
        /// </summary>
        IFormatData FormatData { get; }

        /// <summary>
        /// String array that contains additional meta data and filtering options for UI.
        /// </summary>
        string[] Tags { get; }

        /// <summary>
        /// String array that only contains custom tags applied with the tag attribute.
        /// </summary>
        string[] CustomTags { get; }

        /// <summary>
        /// The monitoring profiler caches every additional attribute that inherits from MonitoringMetaAttribute on
        /// the profile. You can access these custom attributes during runtime using this method without the need of
        /// reflection code.
        /// </summary>
        TAttribute GetMetaAttribute<TAttribute>() where TAttribute : MonitoringMetaAttribute;

        /// <summary>
        /// Try to get a MonitoringMetaAttribute.
        /// </summary>
        bool TryGetMetaAttribute<TAttribute>(out TAttribute attribute) where TAttribute : MonitoringMetaAttribute;
    }
}
using System;
using System.Reflection;
using Baracuda.Monitoring.Attributes;
using Baracuda.Monitoring.Internal.Utils;

namespace Baracuda.Monitoring.Interface
{
    public interface IMonitorProfile
    {
        /// <summary>
        /// Instance of the attribute that is used to flag the target member to be monitored.
        /// </summary>
        MonitorAttribute Attribute { get; }
        MemberInfo MemberInfo { get; }
        UnitType UnitType { get; }
        Segment Segment { get; }
        Type UnitTargetType { get; }
        Type UnitValueType { get; }
        Type UnitDeclaringType { get; }
        string[] Tags { get; }
        bool IsStatic { get; }
        bool ShowIndexer { get; }
        string Label { get; }
        string Format { get; }
        int FontSize { get; }
        int IndentValue { get; }
        string Indent { get; }
        UIPosition Position { get; }
        bool AllowGrouping { get; }
        string[] UssStyles { get; }
        string GroupName { get; }
    }
}
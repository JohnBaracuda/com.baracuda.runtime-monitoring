using System;
using System.Reflection;
using Baracuda.Attributes;
using Baracuda.Attributes.Monitoring;
using Baracuda.Monitoring.Interface;
using Baracuda.Monitoring.Internal.Processing;
using Baracuda.Monitoring.Internal.Reflection;
using Baracuda.Monitoring.Internal.Units;
using Baracuda.Monitoring.Internal.Utils;
using Baracuda.Monitoring.Management;
using Baracuda.Pooling.Concretions;
using UnityEngine;

namespace Baracuda.Monitoring.Internal.Profiles
{
    public abstract class MonitorProfile : IMonitorProfile
    {
        #region --- [PROPERTIES] ---

        public MonitorAttribute Attribute { get; }
        public MemberInfo MemberInfo { get; }
        public UnitType UnitType { get; }
        public Segment Segment { get; }
        public Type UnitTargetType { get; }
        public Type UnitValueType { get; }
        public Type UnitDeclaringType { get; }
        public  string[] Tags { get; }
        public bool IsStatic { get; }
        public bool ShowIndexer { get; } = true;
        public string Label { get; }
        public string Format { get; }
        public int FontSize { get; }
        public int IndentValue { get; }
        public string Indent { get; }
        public UIPosition Position { get; }
        public bool AllowGrouping { get; } = true;
        public string[] UssStyles { get; } = Array.Empty<string>();
        public string GroupName { get; }

        #endregion

        //--------------------------------------------------------------------------------------------------------------

        #region --- [CTOR] ---

        protected MonitorProfile(
            MemberInfo memberInfo,
            MonitorAttribute attribute,
            Type unitTargetType,
            Type unitValueType,
            UnitType unityType,
            MonitorProfileCtorArgs args)
        {
            Attribute = attribute;
            MemberInfo = memberInfo;
            UnitTargetType = unitTargetType;
            UnitValueType = unitValueType;
            UnitDeclaringType = memberInfo.DeclaringType;
            Segment = attribute.Interval;
            IsStatic = args.ReflectedMemberFlags.HasFlagUnsafe(BindingFlags.Static);
            
            var settings = MonitoringSettings.Instance();

            if (memberInfo.TryGetCustomAttribute<MonitorDisplayOptionsAttribute>(out var options))
            {
                Format = options.Format;
                Label = options.Label;
                ShowIndexer = options.ShowIndexer;
                FontSize = Mathf.Clamp((int) options.FontSize, 0, 64);
                Position = options.Position;
                IndentValue = options.ElementIndent;
                Indent = options.ElementIndent >= 0? $"<pos={options.ElementIndent.ToString()}>" : null;
                AllowGrouping = options.GroupElement;
            }

            AllowGrouping = AllowGrouping && (IsStatic ? settings.groupStaticUnits : settings.groupInstanceUnits);
            
            GroupName = settings.humanizeNames? UnitDeclaringType!.Name.Humanize() : UnitDeclaringType!.Name;
            if (UnitDeclaringType.IsGenericType)
            {
                GroupName = UnitDeclaringType.ToGenericTypeString();
            }
            
            
            if (memberInfo.TryGetCustomAttribute<StyleAttribute>(out var styleAttribute))
            {
                UssStyles = styleAttribute.ClassList;
            }
            
            if (Label == null)
            {
                Label = settings.humanizeNames? memberInfo.Name.Humanize(settings.variablePrefixes) : memberInfo.Name;
                
                if (settings.addClassName)
                {
                    Label = $"{unitTargetType.Name.Colorize(settings.classColor)}{settings.appendSymbol.ToString()}{Label}";
                }
            }

            if (MonitoringProfiler.DefaultTypeFormatter.TryGetValue(unitValueType, out var typeFormatter))
            {
                Format ??= typeFormatter;
            }
           
            UnitType = unityType;
            
            // Tags added to the profile can be used to filter active units.
            var tags = ConcurrentListPool<string>.Get();
            tags.Add(Label);
            tags.Add(UnitType.AsString());
            tags.Add(IsStatic ? "Static" : "Instance");
            if (memberInfo.TryGetCustomAttribute<TagAttribute>(out var categoryAttribute))
            {
                tags.AddRange(categoryAttribute.Tags);
            }
            Tags = tags.ToArray();
            ConcurrentListPool<string>.Release(tags);
        }

        #endregion

        //--------------------------------------------------------------------------------------------------------------

        #region --- [FACTORY] ---

        /// <summary>
        /// Creates a <see cref="MonitorUnit"/> with the <see cref="MonitorProfile"/>. 
        /// </summary>
        /// <param name="target">The target of the unit. Null if static</param>
        /// <returns></returns>
        public abstract MonitorUnit CreateUnit(object target);

        #endregion
    }
}
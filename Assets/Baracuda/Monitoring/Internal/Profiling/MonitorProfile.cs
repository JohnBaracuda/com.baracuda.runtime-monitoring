using System;
using System.Collections.Generic;
using System.Reflection;
using Baracuda.Monitoring.Attributes;
using Baracuda.Monitoring.Interface;
using Baracuda.Monitoring.Internal.Pooling.Concretions;
using Baracuda.Monitoring.Internal.Reflection;
using Baracuda.Monitoring.Internal.Units;
using Baracuda.Monitoring.Internal.Utilities;
using UnityEngine;

namespace Baracuda.Monitoring.Internal.Profiling
{
    public abstract class MonitorProfile : IMonitorProfile
    {
        #region --- Interface ---

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
        public string GroupName { get; }
        
        public bool TryGetMetaAttribute<TAttribute>(out TAttribute attribute) where TAttribute : MonitoringMetaAttribute
        {
            var result = _metaAttributes.TryGetValue(typeof(TAttribute), out var metaAttribute);
            attribute = metaAttribute as TAttribute;
            return result;
        }

        #endregion

        #region --- Fields ---
        
        private readonly Dictionary<Type, MonitoringMetaAttribute> _metaAttributes =
            new Dictionary<Type, MonitoringMetaAttribute>();
        
        #endregion
        
        //--------------------------------------------------------------------------------------------------------------

        #region --- Ctor ---

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
            
            var settings = args.Settings;
            
            foreach (var monitoringMetaAttribute in memberInfo.GetCustomAttributes<MonitoringMetaAttribute>())
            {
                _metaAttributes.Add(monitoringMetaAttribute.GetType(), monitoringMetaAttribute);
            }

            if (TryGetMetaAttribute<FormatAttribute>(out var options))
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

            AllowGrouping = AllowGrouping && (IsStatic ? settings.GroupStaticUnits : settings.GroupInstanceUnits);
            
            GroupName = settings.HumanizeNames? UnitDeclaringType!.Name.Humanize() : UnitDeclaringType!.Name;
            if (UnitDeclaringType.IsGenericType)
            {
                GroupName = UnitDeclaringType.ToSyntaxString();
            }
            
            if (Label == null)
            {
                Label = settings.HumanizeNames? memberInfo.Name.Humanize(settings.VariablePrefixes) : memberInfo.Name;
                
                if (settings.AddClassName)
                {
                    Label = $"{unitTargetType.Name.Colorize(settings.ClassColor)}{settings.AppendSymbol.ToString()}{Label}";
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
            tags.Add(UnitType.ToString());
            tags.Add(IsStatic ? "Static" : "Instance");
            if (TryGetMetaAttribute<TagAttribute>(out var categoryAttribute))
            {
                tags.AddRange(categoryAttribute.Tags);
            }
            Tags = tags.ToArray();
            ConcurrentListPool<string>.Release(tags);
        }

        #endregion

        //--------------------------------------------------------------------------------------------------------------

        #region --- Factory ---

        /// <summary>
        /// Creates a <see cref="MonitorUnit"/> with the <see cref="MonitorProfile"/>. 
        /// </summary>
        /// <param name="target">The target of the unit. Null if static</param>
        /// <returns></returns>
        public abstract MonitorUnit CreateUnit(object target);

        #endregion
    }
}
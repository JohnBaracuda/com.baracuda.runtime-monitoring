// Copyright (c) 2022 Jonathan Lang
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Baracuda.Monitoring.Interface;
using Baracuda.Monitoring.Internal.Units;
using Baracuda.Monitoring.Internal.Utilities;
using Baracuda.Pooling.Concretions;
using Baracuda.Reflection;

namespace Baracuda.Monitoring.Internal.Profiling
{
    public abstract class MonitorProfile : IMonitorProfile
    {
        #region --- Interface ---

        public MonitorAttribute Attribute { get; }
        public MemberInfo MemberInfo { get; }
        public UnitType UnitType { get; }
        public UpdateOptions UpdateOptions { get; }
        public bool RequiresUpdate { get; protected set; } = true;
        public Type UnitTargetType { get; }
        public Type UnitValueType { get; }
        public bool IsStatic { get; }
        public string[] Tags { get; }
        public FormatData FormatData { get; }

        public bool TryGetMetaAttribute<TAttribute>(out TAttribute attribute) where TAttribute : MonitoringMetaAttribute
        {
            attribute = GetMetaAttribute<TAttribute>();
            return attribute != null;
        }
        
        public TAttribute GetMetaAttribute<TAttribute>() where TAttribute : MonitoringMetaAttribute
        {
            _metaAttributes.TryGetValue(typeof(TAttribute), out var metaAttribute);
            return metaAttribute as TAttribute;;
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
            UnitType = unityType;
            UpdateOptions = attribute.Update;
#if UNITY_2020_1_OR_NEWER
            IsStatic = args.ReflectedMemberFlags.HasFlagUnsafe(BindingFlags.Static);
#else
            var intFlag = (int) args.ReflectedMemberFlags;
            IsStatic = intFlag.HasFlag32((int) BindingFlags.Static);
#endif
            var settings = args.Settings;
            
            foreach (var monitoringMetaAttribute in memberInfo.GetCustomAttributes<MonitoringMetaAttribute>())
            {
                _metaAttributes.Add(monitoringMetaAttribute.GetType(), monitoringMetaAttribute);
            }

            FormatData = FormatData.Create(this, settings);
            
            var tags = ConcurrentListPool<string>.Get();
            tags.Add(FormatData.Label);
            tags.Add(UnitType.AsString());
            tags.Add(IsStatic ? "Static" : "Instance");
            tags.Add(UnitTargetType.Name);
            tags.Add(UnitValueType.Name.ToTypeKeyWord());
            if (TryGetMetaAttribute<MTagAttribute>(out var categoryAttribute))
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
        internal abstract MonitorUnit CreateUnit(object target);

        #endregion
    }
}
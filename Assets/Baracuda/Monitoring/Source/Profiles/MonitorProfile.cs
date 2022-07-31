// Copyright (c) 2022 Jonathan Lang

using System;
using System.Collections.Generic;
using System.Reflection;
using Baracuda.Monitoring.API;
using Baracuda.Monitoring.Source.Interfaces;
using Baracuda.Monitoring.Source.Types;
using Baracuda.Monitoring.Source.Units;
using Baracuda.Pooling.Concretions;
using Baracuda.Reflection;
using static Baracuda.Monitoring.Source.Types.FormatData;

namespace Baracuda.Monitoring.Source.Profiles
{
    public abstract class MonitorProfile : IMonitorProfile
    {
        #region --- Interface ---
        
        public MonitorAttribute Attribute { get; }
        public MemberInfo MemberInfo { get; }
        public MemberType MemberType { get; }
        public bool ReceiveTick { get; protected set; } = true;
        public bool DefaultEnabled { get; } = true;
        public Type UnitTargetType { get; }
        public Type UnitValueType { get; }
        public bool IsStatic { get; }
        public string[] Tags { get; }
        public IFormatData FormatData { get; }
        

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
            MemberType unityType,
            MonitorProfileCtorArgs args)
        {
            Attribute = attribute;
            MemberInfo = memberInfo;
            UnitTargetType = unitTargetType;
            UnitValueType = unitValueType;
            MemberType = unityType; 

            var intFlag = (int) args.ReflectedMemberFlags;
            IsStatic = intFlag.HasFlag32((int) BindingFlags.Static);

            var settings = args.Settings;
            
            foreach (var metaAttribute in memberInfo.GetCustomAttributes<MonitoringMetaAttribute>(true))
            {
                var key = metaAttribute is MOptionsAttribute ? typeof(MOptionsAttribute) : metaAttribute.GetType();
                if (!_metaAttributes.ContainsKey(key))
                {
                    _metaAttributes.Add(key, metaAttribute);
                }
            }

            var utility = MonitoringSystems.Resolve<IMonitoringUtilityInternal>();
            
            if (TryGetMetaAttribute<MFontNameAttribute>(out var fontAttribute))
            {
                utility.AddFontHash(fontAttribute.FontHash);
            }

            if (TryGetMetaAttribute<MEnabledAttribute>(out var enabledAttribute))
            {
                DefaultEnabled = enabledAttribute.Enabled;
            }
            else if (TryGetMetaAttribute<MOptionsAttribute>(out var optionsAttribute))
            {
                DefaultEnabled = optionsAttribute.Enabled;
            }
            
            FormatData = CreateFormatData(this, settings);
            
            var tags = ConcurrentListPool<string>.Get();

            if(settings.FilterLabel)
            {
                tags.Add(FormatData.Label);
            }
            
            if(settings.FilterMemberType)
            {
                tags.Add(MemberType.AsString());
            }
            
            if(settings.FilterStaticOrInstance)
            {
                tags.Add(IsStatic ? "Static" : "Instance");
            }
            
            if(settings.FilterDeclaringType)
            {
                tags.Add(UnitTargetType.Name);
            }
            
            if(settings.FilterType)
            {
                tags.Add(UnitValueType.Name.ToTypeKeyWord());
            }
            
            if(settings.FilterTags && TryGetMetaAttribute<MTagAttribute>(out var categoryAttribute))
            {
                foreach (var tag in categoryAttribute.Tags)
                {
                    tags.Add(tag);
                    utility.AddTag(tag);
                }
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
        
        //--------------------------------------------------------------------------------------------------------------
        
        #region --- Reflection Fields ---

        protected const BindingFlags STATIC_FLAGS
            = BindingFlags.Default |
              BindingFlags.Static |
              BindingFlags.Public |
              BindingFlags.NonPublic |
              BindingFlags.DeclaredOnly;

        protected const BindingFlags INSTANCE_FLAGS
            = BindingFlags.Default |
              BindingFlags.Public |
              BindingFlags.NonPublic |
              BindingFlags.DeclaredOnly |
              BindingFlags.Instance;

        #endregion

        //--------------------------------------------------------------------------------------------------------------

        #region --- Validator ---

        
        
        #endregion

        #region --- Obsolete ---

#pragma warning disable CS0618
        public UpdateOptions UpdateOptions { get; } = default;
        public MemberType UnitType => MemberType;
#pragma warning restore CS0618

        #endregion
    }
}
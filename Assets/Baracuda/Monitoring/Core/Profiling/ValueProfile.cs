// Copyright (c) 2022 Jonathan Lang

using System;
using System.Collections.Generic;
using System.Reflection;
using Baracuda.Monitoring.Core.Utilities;
using Baracuda.Reflection;

namespace Baracuda.Monitoring.Core.Profiling
{
    public abstract class ValueProfile<TTarget, TValue> : NotifiableProfile<TTarget, TValue> where TTarget : class
    {
        #region --- Properties ---
        
        public bool SetAccessEnabled { get; } = false;
        internal IsDirtyDelegate IsDirtyFunc { get; }
        
        #endregion
        
        #region --- Fields ---
        
        public delegate bool IsDirtyDelegate(ref TValue lastValue, ref TValue newValue);
        
        #endregion

        //--------------------------------------------------------------------------------------------------------------

        #region --- Properties ---

        private readonly Func<TTarget, TValue, string> _instanceValueProcessorDelegate;
        private readonly Func<TValue, string> _staticValueProcessorDelegate;
        private readonly Func<TValue, string> _fallbackValueProcessorDelegate;
        protected MulticastDelegate Validator { get; }
        
        private static readonly EqualityComparer<TValue> comparer = EqualityComparer<TValue>.Default;
        
        protected Func<TValue, string> ValueProcessor(TTarget target)
        {
            return _instanceValueProcessorDelegate != null 
                    ? value => _instanceValueProcessorDelegate(target, value) 
                    : _staticValueProcessorDelegate ?? _fallbackValueProcessorDelegate;
        }

        #endregion

        //--------------------------------------------------------------------------------------------------------------

        protected ValueProfile(
            MemberInfo memberInfo,
            MonitorAttribute attribute,
            Type unitTargetType,
            Type unitValueType, 
            UnitType unitType,
            MonitorProfileCtorArgs args) 
            : base(memberInfo, attribute, unitTargetType, unitValueType, unitType, args)
        {
            if (attribute is MonitorValueAttribute valueAttribute)
            {
                SetAccessEnabled = valueAttribute.EnableSetAccess;
            }
            
            if (TryGetMetaAttribute<MValueProcessorAttribute>(out var valueProcessorAttribute))
            {
                var processorName = valueProcessorAttribute.Processor;
                _instanceValueProcessorDelegate = ValueProcessorFactory.FindCustomInstanceProcessor<TTarget, TValue>(processorName, FormatData);
                _staticValueProcessorDelegate = ValueProcessorFactory.FindCustomStaticProcessor<TTarget, TValue>(processorName, FormatData);
                if (_instanceValueProcessorDelegate == null && _staticValueProcessorDelegate == null)
                {
                    ExceptionLogging.LogValueProcessNotFound(processorName, unitTargetType);
                }
            }
            if (_staticValueProcessorDelegate == null && _instanceValueProcessorDelegate == null)
            {
                _fallbackValueProcessorDelegate = ValueProcessorFactory.CreateProcessorForType<TValue>(FormatData);
            }

            IsDirtyFunc = CreateIsDirtyFunction(unitValueType);
            
            if (TryGetMetaAttribute<MConditionalAttribute>(out var conditionalAttribute))
            {
                Validator = (MulticastDelegate) ValidatorFactory.CreateStaticValidator(conditionalAttribute, unitTargetType)
                            ?? (MulticastDelegate) ValidatorFactory.CreateStaticConditionalValidator<TValue>(conditionalAttribute, unitTargetType)
                            ?? (MulticastDelegate) ValidatorFactory.CreateInstanceValidator<TTarget>(conditionalAttribute);
                            
                //TODO make event add/remove
                //?? (MulticastDelegate) ValidatorFactory.CreateEventValidator(conditionalAttribute, unitTargetType);
            }
        }

        private static IsDirtyDelegate CreateIsDirtyFunction(Type memberType)
        {
            if (memberType.IsValueType)
            {
                return (ref TValue lastValue, ref TValue newValue) => !comparer.Equals(lastValue, newValue);
            }

            if (memberType.IsString())
            {
                return (ref TValue lastValue, ref TValue newValue) => !ReferenceEquals(lastValue, newValue);
            }

            return (ref TValue lastValue, ref TValue newValue) => true;
        }
    }
}
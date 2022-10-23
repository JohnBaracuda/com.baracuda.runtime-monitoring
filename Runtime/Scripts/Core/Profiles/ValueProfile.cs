// Copyright (c) 2022 Jonathan Lang

using Baracuda.Monitoring.Types;
using Baracuda.Monitoring.Utilities.Extensions;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Baracuda.Monitoring.Profiles
{
    internal abstract class ValueProfile<TTarget, TValue> : NotifiableProfile<TTarget, TValue> where TTarget : class
    {
        #region Properties

        public bool SetAccessEnabled { get; }
        internal IsDirtyDelegate IsDirtyFunc { get; }

        #endregion

        #region Fields

        public delegate bool IsDirtyDelegate(ref TValue lastValue, ref TValue newValue);

        #endregion

        #region Properties

        private readonly Func<TTarget, TValue, string> _instanceValueProcessorDelegate;
        private readonly Func<TValue, string> _staticValueProcessorDelegate;
        private readonly Func<TValue, string> _fallbackValueProcessorDelegate;
        protected MulticastDelegate ValidationFunc { get; }
        protected ValidationEvent ValidationEvent { get; }

        private static readonly EqualityComparer<TValue> comparer = EqualityComparer<TValue>.Default;

        protected Func<TValue, string> ValueProcessor(TTarget target)
        {
            return _instanceValueProcessorDelegate != null
                    ? value => _instanceValueProcessorDelegate(target, value)
                    : _staticValueProcessorDelegate ?? _fallbackValueProcessorDelegate;
        }

        #endregion

        #region Ctor

        protected ValueProfile(
            MemberInfo memberInfo,
            MonitorAttribute attribute,
            Type unitTargetType,
            Type unitValueType,
            MemberType memberType,
            MonitorProfileCtorArgs args)
            : base(memberInfo, attribute, unitTargetType, unitValueType, memberType, args)
        {
            if (attribute is MonitorValueAttribute valueAttribute)
            {
                SetAccessEnabled = valueAttribute.EnableSetAccess;
            }

            IsDirtyFunc = CreateIsDirtyFunction(unitValueType);

            // Value Processor

            var valueProcessorName = default(string);

            if (TryGetMetaAttribute<MValueProcessorAttribute>(out var valueProcessorAttribute) &&
                !string.IsNullOrWhiteSpace(valueProcessorAttribute.Processor))
            {
                valueProcessorName = valueProcessorAttribute.Processor;
            }
            else if (TryGetMetaAttribute<MOptionsAttribute>(out var optionsAttribute) &&
                     !string.IsNullOrWhiteSpace(optionsAttribute.ValueProcessor))
            {
                valueProcessorName = optionsAttribute.ValueProcessor;
            }


            if (valueProcessorName != null)
            {
                var valueProcessorFactory = Monitor.ProcessorFactory;
                _instanceValueProcessorDelegate =
                    valueProcessorFactory.FindCustomInstanceProcessor<TTarget, TValue>(valueProcessorName, FormatData);
                _staticValueProcessorDelegate =
                    valueProcessorFactory.FindCustomStaticProcessor<TTarget, TValue>(valueProcessorName, FormatData);
                if (_instanceValueProcessorDelegate == null && _staticValueProcessorDelegate == null)
                {
                    Monitor.Logger.LogValueProcessNotFound(valueProcessorName, unitTargetType);
                }
            }

            if (_staticValueProcessorDelegate == null && _instanceValueProcessorDelegate == null)
            {
                var valueProcessorFactory = Monitor.ProcessorFactory;
                _fallbackValueProcessorDelegate = valueProcessorFactory.CreateProcessorForType<TValue>(FormatData);
            }

            // Validator

            if (TryGetMetaAttribute<MShowIfAttribute>(out var conditionalAttribute))
            {
                ValidationFunc = (MulticastDelegate) Monitor.ValidatorFactory.CreateStaticValidator(conditionalAttribute, memberInfo)
                                 ?? (MulticastDelegate) Monitor.ValidatorFactory.CreateStaticConditionalValidator<TValue>(conditionalAttribute, memberInfo)
                                 ?? Monitor.ValidatorFactory.CreateInstanceValidator<TTarget>(conditionalAttribute, memberInfo);

                ValidationEvent = Monitor.ValidatorFactory.CreateEventValidator(conditionalAttribute, memberInfo);
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

        #endregion
    }
}
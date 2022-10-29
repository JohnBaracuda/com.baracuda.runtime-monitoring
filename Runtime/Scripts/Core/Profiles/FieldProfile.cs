// Copyright (c) 2022 Jonathan Lang

using Baracuda.Monitoring.Types;
using Baracuda.Monitoring.Units;
using Baracuda.Monitoring.Utilities.Extensions;
using System;
using System.Reflection;

namespace Baracuda.Monitoring.Profiles
{
    /// <typeparam name="TTarget">The <see cref="Type"/> of the fields target object</typeparam>
    /// <typeparam name="TValue">The <see cref="Type"/> of the return value of the field</typeparam>
    internal sealed class FieldProfile<TTarget, TValue> : ValueProfile<TTarget, TValue> where TTarget : class
    {
        private readonly Func<TTarget, TValue> _getValueDelegate;
        private readonly Action<TTarget, TValue> _setValueDelegate;

        /// <summary>
        /// Create a new <see cref="FieldHandle{TTarget,TValue}"/> based on this profile.
        /// </summary>
        /// <param name="target">Target object for the unit. Null if it is a static unit.</param>
        internal override MonitorHandle CreateUnit(object target)
        {
            return new FieldHandle<TTarget, TValue>(
                (TTarget) target,
                _getValueDelegate,
                _setValueDelegate,
                ValueProcessor((TTarget) target),
                ValidationFunc,
                ValidationEvent,
                this);
        }

        private FieldProfile(FieldInfo fieldInfo, MonitorAttribute attribute, MonitorProfileCtorArgs args)
            : base(fieldInfo, attribute, typeof(TTarget), typeof(TValue), MemberType.Field, args)
        {
            _getValueDelegate = fieldInfo.CreateGetter<TTarget, TValue>();
            _setValueDelegate = SetAccessEnabled
                ? fieldInfo.CreateSetter<TTarget, TValue>()
                : null;
        }
    }
}
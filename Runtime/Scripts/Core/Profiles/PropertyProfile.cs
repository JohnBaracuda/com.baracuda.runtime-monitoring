// Copyright (c) 2022 Jonathan Lang

using Baracuda.Monitoring.Types;
using Baracuda.Monitoring.Units;
using Baracuda.Monitoring.Utilities.Extensions;
using System;
using System.Reflection;

namespace Baracuda.Monitoring.Profiles
{
    /// <typeparam name="TTarget">The Type of the properties target object</typeparam>
    /// <typeparam name="TValue">The Type of the properties return value</typeparam>
    internal sealed class PropertyProfile<TTarget, TValue> : ValueProfile<TTarget, TValue> where TTarget : class
    {
        private readonly Func<TTarget, TValue> _getValueDelegate;
        private readonly Action<TTarget, TValue> _setValueDelegate;

        /// <summary>
        /// Create a new <see cref="PropertyHandle{TTarget,TValue}"/> based on this profile.
        /// </summary>
        /// <param name="target">Target object for the unit. Null if it is a static unit.</param>
        internal override MonitorHandle CreateUnit(object target)
        {
            return new PropertyHandle<TTarget, TValue>(
                (TTarget) target,
                _getValueDelegate,
                _setValueDelegate,
                ValueProcessor((TTarget) target),
                ValidationFunc,
                ValidationEvent,
                this);
        }

        private PropertyProfile(
            PropertyInfo propertyInfo,
            MonitorAttribute attribute,
            MonitorProfileCtorArgs args) : base(propertyInfo, attribute, typeof(TTarget), typeof(TValue), MemberType.Property, args)
        {
#if !ENABLE_IL2CPP
            var backField = propertyInfo.GetBackingField();

            _getValueDelegate = backField?.CreateGetter<TTarget, TValue>() ?? CreateGetDelegate(propertyInfo.GetMethod);
            _setValueDelegate = SetAccessEnabled
                ? backField?.CreateSetter<TTarget, TValue>() ?? CreateSetDelegate(propertyInfo.SetMethod)
                : null;
#else
            _getValueDelegate = CreateGetDelegate(propertyInfo.GetMethod);
            _setValueDelegate = SetAccessEnabled
                ? CreateSetDelegate(propertyInfo.SetMethod)
                : null;
#endif
        }


        private static Func<TTarget, TValue> CreateGetDelegate(MethodInfo methodInfo)
        {
            return target => (TValue) methodInfo.Invoke(target, null);
        }

        private static Action<TTarget, TValue> CreateSetDelegate(MethodInfo methodInfo)
        {
            var proxy = new object[1];
            return (target, value) =>
            {
                proxy[0] = value;
                methodInfo.Invoke(target, proxy);
            };
        }
    }
}
// Copyright (c) 2022 Jonathan Lang
using System;
using System.Reflection;
using Baracuda.Monitoring.Internal.Units;
using Baracuda.Monitoring.Internal.Utilities;
using Baracuda.Reflection;

namespace Baracuda.Monitoring.Internal.Profiling
{
    /// <typeparam name="TTarget">The Type of the properties target object</typeparam>
    /// <typeparam name="TValue">The Type of the properties return value</typeparam>
    public sealed class PropertyProfile<TTarget, TValue> : ValueProfile<TTarget, TValue> where TTarget : class
    {
        #region --- Fields ---

        private readonly Func<TTarget, TValue> _getValueDelegate;
        private readonly Action<TTarget, TValue> _setValueDelegate;
        
        #endregion
        
        //--------------------------------------------------------------------------------------------------------------
        
        #region --- Factory ---

        /// <summary>
        /// Create a new <see cref="PropertyUnit{TTarget, TValue}"/> based on this profile.
        /// </summary>
        /// <param name="target">Target object for the unit. Null if it is a static unit.</param>
        internal override MonitorUnit CreateUnit(object target)
        {
            return new PropertyUnit<TTarget, TValue>(
                (TTarget)target,
                _getValueDelegate,
                _setValueDelegate,
                ValueProcessor((TTarget)target),
                this);
        }

        #endregion

        //--------------------------------------------------------------------------------------------------------------
        
        #region --- Ctor ---
        
        private PropertyProfile(
            PropertyInfo propertyInfo,
            MonitorAttribute attribute,
            MonitorProfileCtorArgs args) : base(propertyInfo, attribute, typeof(TTarget), typeof(TValue), UnitType.Property, args)
        {
#if !ENABLE_IL2CPP 
            var backField = propertyInfo.GetBackingField();
            
            _getValueDelegate = backField?.CreateGetter<TTarget, TValue>() ?? CreateGetDelegate(propertyInfo.GetMethod);
            _setValueDelegate = SetAccessEnabled
                ? backField?.CreateSetter<TTarget, TValue>() ?? CreateSetDelegate(propertyInfo.SetMethod) 
                : null;
#else
            _getValueDelegate = CreateGetDelegate(propertyInfo.GetMethod);
            _setValueDelegate =  SetAccessEnabled
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

        #endregion
        
    }
}
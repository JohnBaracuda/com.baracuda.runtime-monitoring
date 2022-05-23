// Copyright (c) 2022 Jonathan Lang
using System;
using System.Reflection;
using Baracuda.Monitoring.Internal.Units;
using Baracuda.Monitoring.Internal.Utilities;
using Baracuda.Reflection;

namespace Baracuda.Monitoring.Internal.Profiling
{
    /// <typeparam name="TTarget">The <see cref="Type"/> of the fields target object</typeparam>
    /// <typeparam name="TValue">The <see cref="Type"/> of the return value of the field</typeparam>
    public sealed class FieldProfile<TTarget, TValue> : ValueProfile<TTarget, TValue> where TTarget : class
    {
        #region --- Fields ---

        private readonly Func<TTarget, TValue> _getValueDelegate;
        private readonly Action<TTarget, TValue> _setValueDelegate;
        
        #endregion
        
        //--------------------------------------------------------------------------------------------------------------
        
        #region --- Factory ---

        /// <summary>
        /// Create a new <see cref="FieldUnit{TTarget, TValue}"/> based on this profile.
        /// </summary>
        /// <param name="target">Target object for the unit. Null if it is a static unit.</param>
        internal override MonitorUnit CreateUnit(object target)
        {
            return new FieldUnit<TTarget, TValue>(
                (TTarget)target,
                _getValueDelegate,
                _setValueDelegate,
                ValueProcessor((TTarget)target),
                this);
        }

        #endregion
        
        //--------------------------------------------------------------------------------------------------------------

        #region --- Ctor ---
        
        private FieldProfile(FieldInfo fieldInfo, MonitorAttribute attribute, MonitorProfileCtorArgs args) 
            : base(fieldInfo, attribute, typeof(TTarget), typeof(TValue), UnitType.Field, args)
        {
            _getValueDelegate = fieldInfo.CreateGetter<TTarget, TValue>();
            _setValueDelegate = SetAccessEnabled
                ? fieldInfo.CreateSetter<TTarget, TValue>()
                : null;
        }

        #endregion

    }
}
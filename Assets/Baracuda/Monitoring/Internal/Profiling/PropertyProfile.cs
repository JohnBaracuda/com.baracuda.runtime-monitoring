using System;
using System.Reflection;
using Baracuda.Monitoring.Attributes;
using Baracuda.Monitoring.Internal.Exceptions;
using Baracuda.Monitoring.Internal.Reflection;
using Baracuda.Monitoring.Internal.Units;
using Baracuda.Monitoring.Internal.Utilities;

namespace Baracuda.Monitoring.Internal.Profiling
{
    /// <typeparam name="TTarget">The <see cref="Type"/> of the property target object</typeparam>
    /// <typeparam name="TValue">The <see cref="Type"/> of the return value of the property</typeparam>
    public sealed class PropertyProfile<TTarget, TValue> : ValueProfile<TTarget, TValue> where TTarget : class
    {
        #region --- Fields ---

        private readonly Func<TTarget, TValue> _getValueDelegate;
        private readonly Action<TTarget, TValue> _setValueDelegate;
        
        #endregion
        
        //--------------------------------------------------------------------------------------------------------------
        
        #region --- Factory ---

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
#if !ENABLE_IL2CPP // backing field access is not allowed in IL2CPP
            if (attribute is MonitorPropertyAttribute propertyAttribute)
            {
                var getBacking = propertyAttribute.GetBacking || propertyAttribute.TargetBacking;
                var setBacking = propertyAttribute.SetBacking || propertyAttribute.TargetBacking;

                if (getBacking || setBacking)
                {
                    var backField = propertyInfo.GetBackingField();
                    if (backField == null)
                    {
                        ExceptionLogging.LogException(new BackfieldNotFoundException(propertyInfo));
                        return;
                    }
                    
                    if(getBacking)
                    {
                        _getValueDelegate = backField.CreateGetter<TTarget, TValue>();
                    }

                    if(setBacking)
                    {
                        _setValueDelegate = backField.CreateSetter<TTarget, TValue>();
                    }
                }
            }

            _getValueDelegate ??= CreateGetExpression(propertyInfo.GetMethod);
            _setValueDelegate ??= CreateSetExpression(propertyInfo.SetMethod);
#else
            _getValueDelegate = CreateGetExpression(propertyInfo.GetMethod);
            _setValueDelegate = CreateSetExpression(propertyInfo.SetMethod);
#endif
        }
        

        private static Func<TTarget, TValue> CreateGetExpression(MethodInfo methodInfo)
        {
            return target => (TValue) methodInfo.Invoke(target, null);
        }
        
        private static Action<TTarget, TValue> CreateSetExpression(MethodInfo methodInfo)
        {
            return (target, value) => methodInfo.Invoke(target, new object[] {value});
        }

        #endregion
        
    }
}
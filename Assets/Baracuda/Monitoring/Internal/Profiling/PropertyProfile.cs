using System;
using System.Linq.Expressions;
using System.Reflection;
using Baracuda.Monitoring.Attributes;
using Baracuda.Monitoring.Internal.Exceptions;
using Baracuda.Monitoring.Internal.Units;
using Baracuda.Monitoring.Internal.Utils;
using Baracuda.Reflection;

namespace Baracuda.Monitoring.Internal.Profiling
{
    /// <typeparam name="TTarget">The <see cref="Type"/> of the property target object</typeparam>
    /// <typeparam name="TValue">The <see cref="Type"/> of the return value of the property</typeparam>
    public sealed class PropertyProfile<TTarget, TValue> : ValueProfile<TTarget, TValue> where TTarget : class
    {
        
        #region --- [FIELDS] ---

        private readonly Func<TTarget, TValue> _getValueDelegate;
        private readonly Action<TTarget, TValue> _setValueDelegate;
        private readonly Func<TValue, string> _valueProcessorDelegate;

        
        #endregion
        
        //--------------------------------------------------------------------------------------------------------------
        
        #region --- [FACTORY] ---

        public override MonitorUnit CreateUnit(object target)
        {
            return new PropertyUnit<TTarget, TValue>(
                (TTarget)target,
                _getValueDelegate,
                _setValueDelegate,
                _valueProcessorDelegate,
                this);
        }

        #endregion

        //--------------------------------------------------------------------------------------------------------------
        
        #region --- [CTOR] ---
        
        
        private PropertyProfile(
            PropertyInfo propertyInfo,
            MonitorValueAttribute attribute,
            MonitorProfileCtorArgs args) : base(propertyInfo, attribute, typeof(TTarget), typeof(TValue), UnitType.Property, args)
        {
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
                    
                    if(getBacking) _getValueDelegate = backField.CreateGetter<TTarget, TValue>();
                    if(setBacking) _setValueDelegate = backField.CreateSetter<TTarget, TValue>();
                }
            }

            _getValueDelegate ??= CreateGetExpression(propertyInfo.GetMethod).Compile();
            _setValueDelegate ??= CreateSetExpression(propertyInfo.SetMethod).Compile();
            
            _valueProcessorDelegate = 
                ValueProcessor.FindCustomProcessor(propertyInfo.GetCustomAttribute<ValueProcessorAttribute>()?.Processor, this) 
                ?? ValueProcessor.CreateTypeSpecificProcessor<TValue>(this);
        }
        

        private static Expression<Func<TTarget, TValue>> CreateGetExpression(MethodInfo methodInfo)
        {
            return target => (TValue) methodInfo.Invoke(target, null);
        }
        
        private static Expression<Action<TTarget, TValue>> CreateSetExpression(MethodInfo methodInfo)
        {
            return (target, value) => methodInfo.Invoke(target, new object[] {value});
        }

        #endregion
        
            
    }
}
using System;
using System.Reflection;
using Baracuda.Monitoring.Attributes;
using Baracuda.Monitoring.Internal.Units;
using Baracuda.Monitoring.Internal.Utils;
using Baracuda.Reflection;

namespace Baracuda.Monitoring.Internal.Profiling
{
    /// <typeparam name="TTarget">The <see cref="Type"/> of the fields target object</typeparam>
    /// <typeparam name="TValue">The <see cref="Type"/> of the return value of the field</typeparam>
    public sealed class FieldProfile<TTarget, TValue> : ValueProfile<TTarget, TValue> where TTarget : class 
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
            return new FieldUnit<TTarget, TValue>(
                (TTarget)target,
                
                _getValueDelegate,
                _setValueDelegate,
                _valueProcessorDelegate,
                this);
        }

        #endregion
        
        //--------------------------------------------------------------------------------------------------------------

        #region --- [CTOR] ---
        
        private FieldProfile(FieldInfo fieldInfo, MonitorAttribute attribute, MonitorProfileCtorArgs args) 
            : base(fieldInfo, attribute, typeof(TTarget), typeof(TValue), UnitType.Field, args)
        {
            _getValueDelegate = fieldInfo.CreateGetter<TTarget, TValue>();
            _setValueDelegate = fieldInfo.CreateSetter<TTarget, TValue>();
            
            _valueProcessorDelegate = 
                ValueProcessor.FindCustomProcessor(fieldInfo.GetCustomAttribute<ValueProcessorAttribute>()?.Processor, this) 
                ?? ValueProcessor.CreateTypeSpecificProcessor<TValue>(this);
        }

        #endregion

    }
}
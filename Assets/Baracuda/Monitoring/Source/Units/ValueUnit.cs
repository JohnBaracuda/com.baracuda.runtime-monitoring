// Copyright (c) 2022 Jonathan Lang

using System;
using System.Runtime.CompilerServices;
using Baracuda.Monitoring.API;
using Baracuda.Monitoring.Source.Interfaces;
using Baracuda.Monitoring.Source.Profiles;
using Baracuda.Monitoring.Source.Types;
using UnityEngine;

namespace Baracuda.Monitoring.Source.Units
{
    /// <summary>
    /// Base type for units that monitor a value <br/>
    /// <see cref="FieldUnit{TTarget,TValue}"/><br/>
    /// <see cref="PropertyUnit{TTarget,TValue}"/>
    /// </summary>
    /// <typeparam name="TTarget"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    public abstract class ValueUnit<TTarget, TValue> : MonitorUnit, ISettableValue<TValue>, IGettableValue<TValue> where TTarget : class
    {
        #region --- Fields ---
        
        protected readonly StringDelegate CompiledValueProcessor;
        
        private readonly TTarget _target;
        private readonly Func<TTarget, TValue> _getValue;       
        private readonly Action<TTarget, TValue> _setValue;

        private readonly Action _validationTick;
        private readonly Func<bool> _validateFunc;
        private readonly ValidationEvent _validationEvent;

        private readonly ValueProfile<TTarget,TValue>.IsDirtyDelegate _checkIsDirty;

        private TValue _lastValue = default;

        #endregion
        
        //--------------------------------------------------------------------------------------------------------------

        #region --- Ctor ---

        internal ValueUnit(TTarget target,
            Func<TTarget, TValue> getValue,
            Action<TTarget, TValue> setValue,
            Func<TValue, string> valueProcessor,
            MulticastDelegate validationFunc,
            ValidationEvent validationEvent,
            ValueProfile<TTarget, TValue> profile) : base(target, profile)
        {
            _target = target;
            _getValue = getValue;
            _setValue = setValue;
            
            CompiledValueProcessor = CompileValueProcessor(valueProcessor);

            _checkIsDirty = profile.IsDirtyFunc;
            
            if (profile.CustomUpdateEventAvailable)
            {
                if (!profile.TrySubscribeToUpdateEvent(target, Refresh, SetValue))
                {
                    Debug.LogWarning($"Could not subscribe {Name} to update event!");
                }
            }

            if (validationEvent != null)
            {
                _validationEvent = validationEvent;
                _validationEvent.AddMethod(SetEnabled);
            }
            
            // Prefer event based validation
            else if (validationFunc != null)
            {
                switch (validationFunc)
                {
                    case Func<TTarget, bool> instanceValidator:  
                        _validateFunc = () => instanceValidator(_target);
                        break;
                    
                    case Func<bool> simpleValidator:
                        _validateFunc = simpleValidator;
                        break;
                    
                    case Func<TValue, bool> conditionalValidator: 
                        _validateFunc = () => conditionalValidator(GetValue());
                        break;
                }
                
                _validationTick = () => Enabled = _validateFunc();
                MonitoringSystems.Resolve<IMonitoringTicker>().AddValidationTicker(_validationTick);
            }
        }

        #endregion
                
        //--------------------------------------------------------------------------------------------------------------

        #region --- Value Processor ---
        
        private StringDelegate CompileValueProcessor(Func<TValue, string> func)
        {
            return () => func(_getValue(_target)) ?? NULL;
        }

        #endregion
        
        //--------------------------------------------------------------------------------------------------------------

        #region --- Update ---

        public override void Refresh()
        {
            var current = GetValue();
            
            if (_checkIsDirty(ref current, ref _lastValue))
            {
                var state = GetState();
                RaiseValueChanged(state);
            }

            _lastValue = current;
        }

        #endregion

        #region --- Validation ---

        private void SetEnabled(bool enabled)
        {
            Enabled = enabled;
        }
        
        #endregion
        
        //--------------------------------------------------------------------------------------------------------------
        
        #region --- Get ---

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override string GetState()
        {
            return CompiledValueProcessor();
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TValue GetValue()
        {
            return _getValue(_target);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T GetValueAs<T>()
        {
            return _getValue(_target).ConvertFast<TValue, T>();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public object GetValueAsObject()
        {
            return _getValue(_target);
        }

        #endregion
        
        #region --- Set ---
   
        public void SetValue(TValue value)
        {
            _setValue?.Invoke(_target, value);            
            _lastValue = value;
            var state = GetState();
            RaiseValueChanged(state);
        }

        public void SetValue(object value)
        {
            _setValue?.Invoke(_target, (TValue) value);
            _lastValue = (TValue) value;
            var state = GetState();
            RaiseValueChanged(state);
        }

        public void SetValueStruct<TStruct>(TStruct value) where TStruct : struct
        {
            var converted = value.ConvertFast<TStruct, TValue>();
            _setValue?.Invoke(_target, converted);
            _lastValue = converted;
            var state = GetState();
            RaiseValueChanged(state);
        }
        
        #endregion
        
        //--------------------------------------------------------------------------------------------------------------

        #region --- IDisosable ---

        public override void Dispose()
        {
            base.Dispose();
            
            ((ValueProfile<TTarget, TValue>)Profile).TryUnsubscribeFromUpdateEvent(_target, Refresh, SetValue);
            
            _validationEvent?.RemoveMethod(SetEnabled);
            
            if (_validationTick != null)
            {
                MonitoringSystems.Resolve<IMonitoringTicker>().RemoveValidationTicker(_validationTick);
            }
        }

        #endregion
    }
}
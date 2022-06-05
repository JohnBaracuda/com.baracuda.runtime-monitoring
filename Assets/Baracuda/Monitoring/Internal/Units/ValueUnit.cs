// Copyright (c) 2022 Jonathan Lang

using System;
using System.Runtime.CompilerServices;
using Baracuda.Monitoring.Interface;
using Baracuda.Monitoring.Internal.Profiling;
using Baracuda.Monitoring.Internal.Utilities;
using UnityEngine;

namespace Baracuda.Monitoring.Internal.Units
{
    /// <summary>
    /// Base type for units that monitor a value <br/>
    /// <see cref="FieldUnit{TTarget,TValue}"/><br/>
    /// <see cref="PropertyUnit{TTarget,TValue}"/>
    /// </summary>
    /// <typeparam name="TTarget"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    public abstract class ValueUnit<TTarget, TValue> : MonitorUnit, IValueUnit<TValue> where TTarget : class
    {
        #region --- Fields ---
        
        protected readonly StringDelegate CompiledValueProcessor;
        
        private readonly TTarget _target;
        private readonly Func<TTarget, TValue> _getValue;       
        private readonly Action<TTarget, TValue> _setValue;
        private readonly ValueProfile<TTarget,TValue>.IsDirtyDelegate _checkIsDirty;
        
        private TValue _lastValue = default;

        #endregion
        
        //--------------------------------------------------------------------------------------------------------------

        #region --- Ctor ---

        internal ValueUnit(TTarget target,
            Func<TTarget, TValue> getValue,
            Action<TTarget, TValue> setValue,
            Func<TValue, string> valueProcessor,
            ValueProfile<TTarget, TValue> valueProfile) : base(target, valueProfile)
        {
            _target = target;
            _getValue = getValue;
            _setValue = setValue;
            
            CompiledValueProcessor = CompileValueProcessor(valueProcessor);

            _checkIsDirty = valueProfile.IsDirtyFunc;
            
            if (valueProfile.CustomUpdateEventAvailable)
            {
                if (!valueProfile.TrySubscribeToUpdateEvent(target, Refresh, SetValue))
                {
                    Debug.LogWarning($"Could not subscribe {Name} to update event!");
                }
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
        public T GetValueConverted<T>()
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
        
        public void SetValue<T>(T value) where T : unmanaged
        {
            var converted = value.ConvertFast<T, TValue>();
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
        }

        #endregion
    }
}
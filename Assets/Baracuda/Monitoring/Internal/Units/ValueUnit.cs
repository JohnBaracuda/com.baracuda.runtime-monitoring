// Copyright (c) 2022 Jonathan Lang (CC BY-NC-SA 4.0)
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Baracuda.Monitoring.Interface;
using Baracuda.Monitoring.Internal.Profiling;
using Baracuda.Monitoring.Internal.Utilities;
using Baracuda.Threading;
using Unity.Collections.LowLevel.Unsafe;
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
    public abstract class ValueUnit<TTarget, TValue> : MonitorUnit, IValueUnit where TTarget : class
    {
        #region --- Fields ---
        
        protected readonly StringDelegate CompiledValueProcessor;
        
        private readonly TTarget _target;
        private readonly Func<TTarget, TValue> _getValue;       
        private readonly Action<TTarget, TValue> _setValue;
        private readonly bool _isValueType;

        /*
         * Comparison   
         */
        
        private static readonly EqualityComparer<TValue> comparer = EqualityComparer<TValue>.Default;
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
            
            _isValueType = typeof(TValue).IsValueType;
            
            CompiledValueProcessor = CompileValueProcessor(valueProcessor);
            
            if (valueProfile.CustomUpdateEventAvailable)
            {
                var subscribed = valueProfile.TrySubscribeToUpdateEvent(target, Refresh, SetValue);
                if (subscribed)
                {
                    ExternalUpdateRequired = false;
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

            if (_isValueType)
            {
                if (!comparer.Equals(current, _lastValue))
                {
                    RaiseValueChanged(GetStateFormatted);
                }
            }
            else
            {
                RaiseValueChanged(GetStateFormatted);
            }

            _lastValue = current;
        }

        #endregion
        
        //--------------------------------------------------------------------------------------------------------------
        
        #region --- Get ---
        
        public override string GetStateFormatted
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => CompiledValueProcessor();
        }

        public override string GetStateRaw
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _getValue(_target).ToString();
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T GetValue<T>()
        {
            return _getValue(_target).ConvertFast<TValue, T>();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TValue GetValue()
        {
            return _getValue(_target);
        }

        #endregion
        
        #region --- Set ---
   
        public void SetValue(TValue value)
        {
            _setValue?.Invoke(_target, value);            
            RaiseValueChanged(GetStateFormatted);
        }

        public void SetValue(object value)
        {
            _setValue?.Invoke(_target, (TValue) value);
            RaiseValueChanged(GetStateFormatted);
        }
        
        public void SetValue<T>(T value) where T : unmanaged
        {
            _setValue?.Invoke(_target,  value.ConvertFast<T,TValue>());
            RaiseValueChanged(GetStateFormatted);
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
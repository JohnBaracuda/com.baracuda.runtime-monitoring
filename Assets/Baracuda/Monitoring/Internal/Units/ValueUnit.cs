using System;
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
        private readonly Func<TTarget, TValue> _getValueDelegate;       
        private readonly Action<TTarget, TValue> _setValueDelegate;
        private readonly bool _isValueType;

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
            _getValueDelegate = getValue;
            _setValueDelegate = setValue;
            
            _isValueType = typeof(TValue).IsValueType;
            
            CompiledValueProcessor = CompileValueProcessor(valueProcessor);
            
            if (valueProfile.CustomUpdateEventAvailable)
            {
                var subscribed = valueProfile.TrySubscribeToUpdateEvent(target, Refresh, SetValue);
                if (subscribed)
                {
                    ExternalUpdateRequired = false;
                }
                else
                {
                    Debug.Log("Failed");
                }
            }
        }

        #endregion
                
        //--------------------------------------------------------------------------------------------------------------

        #region --- Value Processor ---
        
        private StringDelegate CompileValueProcessor(Func<TValue, string> func)
        {
            return () => func(_getValueDelegate(_target)) ?? NULL;
        }


        #endregion
        
        //--------------------------------------------------------------------------------------------------------------

        #region --- Update ---

        public override void Refresh()
        {
            RaiseValueChanged(GetValueFormatted);
        }

        #endregion
        
        //--------------------------------------------------------------------------------------------------------------
        
        #region --- Get ---
        
        public override string GetValueFormatted
        {
#if MONITORING_DEBUG
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                Dispatcher.GuardAgainstIsNotMainThread(nameof(GetValueFormatted));
                return CompiledValueProcessor();
            }
#else
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => CompiledValueProcessor();
#endif
        }

        public override string GetValueRaw
        {
#if MONITORING_DEBUG
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                Dispatcher.GuardAgainstIsNotMainThread(nameof(GetValueRaw));
                return _getValueDelegate(_target).ToString();
            }
#else
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _getValueDelegate(_target).ToString();
#endif
        }

        #endregion
        
        #region --- Set ---
   
        public void SetValue(TValue value)
        {
            _setValueDelegate?.Invoke(_target, value);            
            RaiseValueChanged(GetValueFormatted);
        }
        
        public void SetValue(object value)
        {
            _setValueDelegate?.Invoke(_target, (TValue) value);
            RaiseValueChanged(GetValueFormatted);
        }
        
#if UNITY_2020_1_OR_NEWER
        public void SetValue<T>(T value) where T : unmanaged
        {
            _setValueDelegate?.Invoke(_target, _isValueType ? UnsafeUtility.As<T, TValue>(ref value) : value.ConvertUnsafe<T,TValue>());
            RaiseValueChanged(GetValueFormatted);
        }
#endif
        
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
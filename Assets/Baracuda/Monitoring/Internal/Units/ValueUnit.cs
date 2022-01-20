using System;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using Baracuda.Monitoring.Interface;
using Baracuda.Monitoring.Internal.Profiling;
using Baracuda.Monitoring.Internal.Utils;
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
        
        #region --- [FIELDS] ---
        
        protected readonly StringDelegate ValueProcessor;
        
        private readonly TTarget _target;
        private readonly Func<TTarget, TValue> _getValueDelegate;       
        private readonly Action<TTarget, TValue> _setValueDelegate;
        private readonly bool _isValueType;

        #endregion
        
        //--------------------------------------------------------------------------------------------------------------

        #region --- [CTOR] ---

        internal ValueUnit(TTarget target,
            Func<TTarget, TValue> getValue,
            Action<TTarget, TValue> setValue,
            Func<TValue, string> customValueProcessor,
            ValueProfile<TTarget, TValue> valueProfile) : base(target, valueProfile)
        {
            _target = target;
            _getValueDelegate = getValue;
            _setValueDelegate = setValue;
            
            _isValueType = typeof(TValue).IsValueType;

            ValueProcessor = CreateValueProcessor(customValueProcessor);
            
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

        #region --- [VALUE PROCESSOR] ---

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private StringDelegate CreateValueProcessor(Func<TValue, string> processor)
        {
            if(processor != null)
            {
                return CreateTypeSpecificProcessor(processor).Compile(false);
            }
            return () => _getValueDelegate(_target)?.ToString() ?? NULL;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private Expression<StringDelegate> CreateTypeSpecificProcessor(Func<TValue, string> func)
        {
            return () => func(_getValueDelegate(_target)) ?? NULL;
        }

        #endregion
        
        //--------------------------------------------------------------------------------------------------------------

        #region --- [UPDATE] ---

        public override void Refresh()
        {
            RaiseValueChanged(GetValueFormatted);
        }

        #endregion
        
        //--------------------------------------------------------------------------------------------------------------
        
        #region --- [GET] ---
        
        public override string GetValueFormatted
        {
#if MONITORING_DEBUG
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                if (!Dispatcher.IsMainThread())
                {
                    throw new InvalidOperationException($"{nameof(GetValueFormatted)} is only allowed to be called from the main thread!");
                }
                return ValueProcessor();
            }
#else
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => ValueProcessor();
#endif
        }

        public override string GetValueRaw
        {
#if MONITORING_DEBUG
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                if (!Dispatcher.IsMainThread())
                {
                    throw new InvalidOperationException($"{nameof(GetValueRaw)} is only allowed to be called from the main thread!");
                }
                return _getValueDelegate(_target).ToString();
            }
#else
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _getValueDelegate(_target).ToString();
#endif
        }

        #endregion
        
        #region --- [SET] ---
   
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

        #region --- [IDISOSABLE] ---

        public override void Dispose()
        {
            base.Dispose();
            ((ValueProfile<TTarget, TValue>)Profile).TryUnsubscribeFromUpdateEvent(_target, Refresh, SetValue);
        }

        #endregion

    }
}
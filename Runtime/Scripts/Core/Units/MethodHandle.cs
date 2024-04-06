// Copyright (c) 2022 Jonathan Lang

using Baracuda.Monitoring.Profiles;
using Baracuda.Monitoring.Systems;
using Baracuda.Monitoring.Types;
using System;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Baracuda.Monitoring.Units
{
    internal sealed class MethodHandle<TTarget, TValue> : MonitorHandle, IGettableValue<MethodResult<TValue>>
        where TTarget : class
    {
        //--------------------------------------------------------------------------------------------------------------


        #region Fields

        private readonly TTarget _target;
        private readonly Func<TTarget, MethodResult<TValue>> _getValue;

        private readonly StringDelegate _compiledValueProcessor;

        #endregion


        //--------------------------------------------------------------------------------------------------------------


        #region Ctor

        public MethodHandle(
            TTarget target,
            Func<TTarget, MethodResult<TValue>> getValue,
            MethodProfile<TTarget, TValue> profile) : base(target, profile)
        {
            _target = target;
#if DEBUG
            _getValue = value =>
            {
                try
                {
                    return getValue(value);
                }
                catch (Exception exception)
                {
                    MonitoringLogger.Log(
                        $"Exception when calling {nameof(GetValue)} in {this}\n(see next log for more information)",
                        LogType.Warning, false);
                    Monitor.Logger.LogException(exception);
                    Enabled = false;
                    return default(MethodResult<TValue>);
                }
            };
#else
            _getValue = getValue;
#endif

            if (profile.CustomUpdateEventAvailable)
            {
                if (!profile.TrySubscribeToUpdateEvent(target, Refresh, null))
                {
                    Debug.LogWarning($"Could not subscribe {Name} to update event!");
                }
            }

            _compiledValueProcessor = () => _getValue(_target).ToString();
        }

        #endregion


        //--------------------------------------------------------------------------------------------------------------


        #region Update

        public override void Refresh()
        {
            var state = GetState();
            RaiseValueChanged(state);
        }

        #endregion


        //--------------------------------------------------------------------------------------------------------------


        #region Get

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override string GetState()
        {
            return _compiledValueProcessor();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public MethodResult<TValue> GetValue()
        {
            return _getValue(_target);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T GetValueAs<T>()
        {
            return _getValue(_target).Value.ConvertFast<TValue, T>();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override object GetValueAsObject()
        {
            return _getValue(_target);
        }

        #endregion
    }
}
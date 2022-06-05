// Copyright (c) 2022 Jonathan Lang
using System;
using System.Collections.Generic;
using System.Reflection;
using Baracuda.Monitoring.Interface;
using Baracuda.Monitoring.Internal.Utilities;
using Baracuda.Reflection;

namespace Baracuda.Monitoring.Internal.Profiling
{
    public abstract class ValueProfile<TTarget, TValue> : MonitorProfile where TTarget : class
    {
        #region --- Properties ---
        
        public bool SetAccessEnabled { get; } = false;
        
        /// <summary>
        /// When true, the profile was provided with a custom update event and is not required to be evaluated every frame/tick.
        /// </summary>
        internal bool CustomUpdateEventAvailable { get; }
        
        internal IsDirtyDelegate IsDirtyFunc { get; }
        
        #endregion
        
        #region --- Fields ---

        private readonly UpdateHandleDelegate<TTarget, TValue> _addUpdateDelegate; //preferred event type
        private readonly NotifyHandleDelegate<TTarget> _addNotifyDelegate; //event without passed TValue param
        private readonly UpdateHandleDelegate<TTarget, TValue> _removeUpdateDelegate; //preferred event type
        private readonly NotifyHandleDelegate<TTarget> _removeNotifyDelegate; //event without passed TValue param
        
        public delegate bool IsDirtyDelegate(ref TValue lastValue, ref TValue newValue);
        
        #endregion

        //--------------------------------------------------------------------------------------------------------------

        #region --- Properties ---

        private readonly Func<TTarget, TValue, string> _instanceValueProcessorDelegate;
        private readonly Func<TValue, string> _staticValueProcessorDelegate;
        private readonly Func<TValue, string> _fallbackValueProcessorDelegate;
        
        private static readonly EqualityComparer<TValue> comparer = EqualityComparer<TValue>.Default;
        
        protected Func<TValue, string> ValueProcessor(TTarget target)
        {
            return _instanceValueProcessorDelegate != null 
                    ? value => _instanceValueProcessorDelegate(target, value) 
                    : _staticValueProcessorDelegate ?? _fallbackValueProcessorDelegate;
        }

        #endregion

        //--------------------------------------------------------------------------------------------------------------

        protected ValueProfile(
            MemberInfo memberInfo,
            MonitorAttribute attribute,
            Type unitTargetType,
            Type unitValueType, 
            UnitType unitType,
            MonitorProfileCtorArgs args) 
            : base(memberInfo, attribute, unitTargetType, unitValueType, unitType, args)
        {
            if (TryGetMetaAttribute<MUpdateEventAttribute>(out var updateEventAttribute) && !string.IsNullOrWhiteSpace(updateEventAttribute.UpdateEvent))
            {
                _addUpdateDelegate    = CreateUpdateHandlerDelegate<TTarget, TValue>(updateEventAttribute.UpdateEvent, this, true);
                _addNotifyDelegate    = CreateNotifyHandlerDelegate<TTarget>        (updateEventAttribute.UpdateEvent, this, true);
                _removeUpdateDelegate = CreateUpdateHandlerDelegate<TTarget, TValue>(updateEventAttribute.UpdateEvent, this, false);
                _removeNotifyDelegate = CreateNotifyHandlerDelegate<TTarget>        (updateEventAttribute.UpdateEvent, this, false);

                if (_addUpdateDelegate != null || _addNotifyDelegate != null)
                {
                    RequiresUpdate = false;
                }
            }
            
            if (attribute is MonitorValueAttribute valueAttribute)
            {
                SetAccessEnabled = valueAttribute.EnableSetAccess;
                
                //TODO: Obsolete! Remove in 2.0
                #region --- OBSOLETE ---

#pragma warning disable CS0618
                if (RequiresUpdate && !string.IsNullOrWhiteSpace(valueAttribute.UpdateEvent))
                {
                    _addUpdateDelegate =
                        CreateUpdateHandlerDelegate<TTarget, TValue>(valueAttribute.UpdateEvent, this, true);
                    _addNotifyDelegate = CreateNotifyHandlerDelegate<TTarget>(valueAttribute.UpdateEvent, this, true);
                    _removeUpdateDelegate =
                        CreateUpdateHandlerDelegate<TTarget, TValue>(valueAttribute.UpdateEvent, this, false);
                    _removeNotifyDelegate =
                        CreateNotifyHandlerDelegate<TTarget>(valueAttribute.UpdateEvent, this, false);

                    if (_addUpdateDelegate != null || _addNotifyDelegate != null)
                    {
                        RequiresUpdate = false;
                    }
                }
#pragma warning restore CS0618

                #endregion
            }
            
            CustomUpdateEventAvailable = _addUpdateDelegate != null || _addNotifyDelegate != null;
            
            if (TryGetMetaAttribute<MValueProcessorAttribute>(out var valueProcessorAttribute))
            {
                var processorName = valueProcessorAttribute.Processor;
                _instanceValueProcessorDelegate = ValueProcessorFactory.FindCustomInstanceProcessor(processorName, this);
                _staticValueProcessorDelegate = ValueProcessorFactory.FindCustomStaticProcessor(processorName, this);
                if (_instanceValueProcessorDelegate == null && _staticValueProcessorDelegate == null)
                {
                    ExceptionLogging.LogValueProcessNotFound(processorName, unitTargetType);
                }
            }
            if (_staticValueProcessorDelegate == null && _instanceValueProcessorDelegate == null)
            {
                _fallbackValueProcessorDelegate = ValueProcessorFactory.CreateTypeSpecificProcessor<TValue>(this);
            }

            IsDirtyFunc = CreateIsDirtyDelegate(unitValueType);
        }

        private static IsDirtyDelegate CreateIsDirtyDelegate(Type memberType)
        {
            if (memberType.IsValueType)
            {
                return (ref TValue lastValue, ref TValue newValue) => !comparer.Equals(lastValue, newValue);
            }

            if (memberType.IsString())
            {
                return (ref TValue lastValue, ref TValue newValue) => !ReferenceEquals(lastValue, newValue);
            }

            return (ref TValue lastValue, ref TValue newValue) => true;
        }


        internal bool TrySubscribeToUpdateEvent(TTarget target, Action refreshAction, Action<TValue> setValueDelegate)
        {
            if (_addUpdateDelegate != null)
            {
                _addUpdateDelegate(target, setValueDelegate);
                return true;
            }

            if (_addNotifyDelegate != null)
            {
                _addNotifyDelegate(target, refreshAction);
                return true;
            }

            return false;
        }


        internal void TryUnsubscribeFromUpdateEvent(TTarget target, Action notify, Action<TValue> update)
        {
            if (_removeUpdateDelegate != null)
            {
                _removeUpdateDelegate(target, update);
                return;
            }

            _removeNotifyDelegate?.Invoke(target, notify);
        }

        //--------------------------------------------------------------------------------------------------------------

        #region --- Reflection Fields ---

        private const BindingFlags STATIC_FLAGS
            = BindingFlags.Default |
              BindingFlags.Static |
              BindingFlags.Public |
              BindingFlags.NonPublic |
              BindingFlags.DeclaredOnly;

        private const BindingFlags INSTANCE_FLAGS
            = BindingFlags.Default |
              BindingFlags.Public |
              BindingFlags.NonPublic |
              BindingFlags.DeclaredOnly |
              BindingFlags.Instance;

        #endregion

        //--------------------------------------------------------------------------------------------------------------

        #region --- Custom Update Event ---

        private delegate void UpdateHandleDelegate<in T, out TParam>(T target, Action<TParam> listener);
        private delegate void NotifyHandleDelegate<in T>(T target, Action listener);

        private static UpdateHandleDelegate<T, TParam> CreateUpdateHandlerDelegate<T, TParam>(
            string eventName, IMonitorProfile profile, bool createAddMethod)
        {
            // check instance events:
            var instanceEvent = profile.UnitTargetType.GetEvent(eventName, INSTANCE_FLAGS);
            if (instanceEvent != null)
            {
                var method = createAddMethod
                    ? instanceEvent.GetAddMethod(true)
                    : instanceEvent.GetRemoveMethod(true);

                var action =
                    (UpdateHandleDelegate<T, TParam>) Delegate.CreateDelegate(
                        typeof(UpdateHandleDelegate<T, TParam>), method, false);
                if (action != null)
                {
                    return action;
                }
            }


            //------------------------
            var staticEvent = profile.UnitTargetType.GetEvent(eventName, STATIC_FLAGS);
            if (staticEvent != null)
            {
                var method = createAddMethod
                    ? staticEvent.GetAddMethod(true)
                    : staticEvent.GetRemoveMethod(true);

                var action =
                    (Action<Action<TParam>>) Delegate.CreateDelegate(typeof(Action<Action<TParam>>), method, false);
                if (action != null)
                {
                    return (target, value) => action(value);
                }
            }

            return null;
        }

        //--------------------------------------------------------------------------------------------------------------        

        private static NotifyHandleDelegate<T> CreateNotifyHandlerDelegate<T>(string eventName,
            IMonitorProfile profile, bool createAddMethod)
        {
            // check instance events:
            var instanceEvent = profile.UnitTargetType.GetEvent(eventName, INSTANCE_FLAGS);
            if (instanceEvent != null)
            {
                var method = createAddMethod
                    ? instanceEvent.GetAddMethod(true)
                    : instanceEvent.GetRemoveMethod(true);

                var action = (NotifyHandleDelegate<T>) Delegate.CreateDelegate(typeof(NotifyHandleDelegate<T>), method, false);
                if (action != null)
                {
                    return action;
                }
            }


            //------------------------
            var staticEvent = profile.UnitTargetType.GetEvent(eventName, STATIC_FLAGS);
            if (staticEvent != null)
            {
                var method = createAddMethod
                    ? staticEvent.GetAddMethod(true)
                    : staticEvent.GetRemoveMethod(true);

                var action = (Action<Action>) Delegate.CreateDelegate(typeof(Action<Action>), method, false);
                if (action != null)
                {
                    return (target, value) => action(value);
                }
            }

            return null;
        }

        #endregion
    }
}
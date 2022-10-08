// Copyright (c) 2022 Jonathan Lang

using Baracuda.Monitoring.Types;
using System;
using System.Reflection;

namespace Baracuda.Monitoring.Profiles
{
    internal abstract class NotifiableProfile<TTarget, TValue> : MonitorProfile
    {
        /// <summary>
        /// When true, the profile was provided with a custom update event and is not required to be evaluated every frame/tick.
        /// </summary>
        internal bool CustomUpdateEventAvailable { get; }

        private readonly UpdateHandleDelegate<TTarget, TValue> _addUpdateDelegate; //preferred event type
        private readonly NotifyHandleDelegate<TTarget> _addNotifyDelegate; //event without passed TValue param
        private readonly UpdateHandleDelegate<TTarget, TValue> _removeUpdateDelegate; //preferred event type
        private readonly NotifyHandleDelegate<TTarget> _removeNotifyDelegate; //event without passed TValue param

        private delegate void UpdateHandleDelegate<in T, out TParam>(T target, Action<TParam> listener);
        private delegate void NotifyHandleDelegate<in T>(T target, Action listener);

        //--------------------------------------------------------------------------------------------------------------

        #region Ctor ---

        protected NotifiableProfile(
            MemberInfo memberInfo,
            MonitorAttribute attribute,
            Type unitTargetType,
            Type unitValueType,
            MemberType unityType,
            MonitorProfileCtorArgs args) : base(memberInfo, attribute, unitTargetType, unitValueType, unityType, args)
        {
            var updateEventName = default(string);

            if (TryGetMetaAttribute<MUpdateEventAttribute>(out var updateEventAttribute) &&
                !string.IsNullOrWhiteSpace(updateEventAttribute.UpdateEvent))
            {
                updateEventName = updateEventAttribute.UpdateEvent;
            }
            else if (TryGetMetaAttribute<MOptionsAttribute>(out var optionsAttribute) &&
                     !string.IsNullOrWhiteSpace(optionsAttribute.UpdateEvent))
            {
                updateEventName = optionsAttribute.UpdateEvent;
            }

            if (updateEventName != null)
            {
                _addUpdateDelegate = CreateUpdateHandlerDelegate<TTarget, TValue>(updateEventName, this, true);
                _addNotifyDelegate = CreateNotifyHandlerDelegate<TTarget>(updateEventName, this, true);
                _removeUpdateDelegate = CreateUpdateHandlerDelegate<TTarget, TValue>(updateEventName, this, false);
                _removeNotifyDelegate = CreateNotifyHandlerDelegate<TTarget>(updateEventName, this, false);

                if (_addUpdateDelegate != null || _addNotifyDelegate != null)
                {
                    ReceiveTick = false;
                }
            }

            CustomUpdateEventAvailable = _addUpdateDelegate != null || _addNotifyDelegate != null;
        }

        #endregion

        //--------------------------------------------------------------------------------------------------------------

        #region Try Subscribe ---

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

        #endregion

        //--------------------------------------------------------------------------------------------------------------

        #region Custom Update Event ---

        private static UpdateHandleDelegate<T, TParam> CreateUpdateHandlerDelegate<T, TParam>(
            string eventName, IMonitorProfile profile, bool createAddMethod)
        {
            // check instance events:
            var instanceEvent = profile.DeclaringType.GetEvent(eventName, InstanceFlags);
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
            var staticEvent = profile.DeclaringType.GetEvent(eventName, StaticFlags);
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
            var instanceEvent = profile.DeclaringType.GetEvent(eventName, InstanceFlags);
            if (instanceEvent != null)
            {
                var method = createAddMethod
                    ? instanceEvent.GetAddMethod(true)
                    : instanceEvent.GetRemoveMethod(true);

                var action =
                    (NotifyHandleDelegate<T>) Delegate.CreateDelegate(typeof(NotifyHandleDelegate<T>), method, false);
                if (action != null)
                {
                    return action;
                }
            }


            //------------------------
            var staticEvent = profile.DeclaringType.GetEvent(eventName, StaticFlags);
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
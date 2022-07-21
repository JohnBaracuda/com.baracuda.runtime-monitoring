using System;
using System.Reflection;
using Baracuda.Monitoring.Core.Utilities;
using Baracuda.Monitoring.Interface;

namespace Baracuda.Monitoring.Core.Profiling
{
    public abstract class NotifiableProfile<TTarget, TValue> : MonitorProfile
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

        #region --- Ctor ---

        protected NotifiableProfile(
            MemberInfo memberInfo,
            MonitorAttribute attribute,
            Type unitTargetType,
            Type unitValueType,
            UnitType unityType,
            MonitorProfileCtorArgs args) : base(memberInfo, attribute, unitTargetType, unitValueType, unityType, args)
        {
            var hasUpdateEventAttribute = false;

            if (TryGetMetaAttribute<MUpdateEventAttribute>(out var updateEventAttribute) &&
                !string.IsNullOrWhiteSpace(updateEventAttribute.UpdateEvent))
            {
                hasUpdateEventAttribute = true;
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
                if (!hasUpdateEventAttribute && RequiresUpdate &&
#pragma warning disable CS0618
                    !string.IsNullOrWhiteSpace(valueAttribute.UpdateEvent))
                {
                    _addUpdateDelegate =    CreateUpdateHandlerDelegate<TTarget, TValue>(valueAttribute.UpdateEvent, this, true);
                    _addNotifyDelegate =    CreateNotifyHandlerDelegate<TTarget>        (valueAttribute.UpdateEvent, this, true);
                    _removeUpdateDelegate = CreateUpdateHandlerDelegate<TTarget, TValue>(valueAttribute.UpdateEvent, this, false);
                    _removeNotifyDelegate = CreateNotifyHandlerDelegate<TTarget>        (valueAttribute.UpdateEvent, this, false);

                    if (_addUpdateDelegate != null || _addNotifyDelegate != null)
                    {
                        RequiresUpdate = false;
                    }
                }
#pragma warning restore CS0618
            }

            CustomUpdateEventAvailable = _addUpdateDelegate != null || _addNotifyDelegate != null;
        }

        #endregion
        
        //--------------------------------------------------------------------------------------------------------------

        #region --- Try Subscribe ---

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

        #region --- Custom Update Event ---

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

                var action =
                    (NotifyHandleDelegate<T>) Delegate.CreateDelegate(typeof(NotifyHandleDelegate<T>), method, false);
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
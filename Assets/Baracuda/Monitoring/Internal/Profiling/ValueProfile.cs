using System;
using System.Reflection;
using Baracuda.Monitoring.Attributes;
using Baracuda.Monitoring.Internal.Utils;

namespace Baracuda.Monitoring.Internal.Profiling
{
    public abstract class ValueProfile<TTarget, TValue> : MonitorProfile where TTarget : class
    {
        internal readonly bool CustomUpdateEventAvailable;

        private readonly UpdateHandleDelegate<TTarget, TValue> _addUpdateDelegate; //preferred event type
        private readonly NotifyHandleDelegate<TTarget> _addNotifyDelegate; //event without passed TValue param
        private readonly UpdateHandleDelegate<TTarget, TValue> _removeUpdateDelegate; //preferred event type
        private readonly NotifyHandleDelegate<TTarget> _removeNotifyDelegate; //event without passed TValue param

        protected ValueProfile(
            MemberInfo memberInfo,
            MonitorAttribute attribute,
            Type unitTargetType,
            Type unitValueType, 
            UnitType unitType,
            MonitorProfileCtorArgs args) 
            : base(memberInfo, attribute, unitTargetType, unitValueType, unitType, args)
        {
            
            if (attribute is MonitorValueAttribute valueAttribute && !string.IsNullOrWhiteSpace(valueAttribute.Update))
            {
                _addUpdateDelegate    = CreateUpdateHandlerDelegate<TTarget, TValue>(valueAttribute.Update, this, true);
                _addNotifyDelegate    = CreateNotifyHandlerDelegate<TTarget>        (valueAttribute.Update, this, true);
                _removeUpdateDelegate = CreateUpdateHandlerDelegate<TTarget, TValue>(valueAttribute.Update, this, false);
                _removeNotifyDelegate = CreateNotifyHandlerDelegate<TTarget>        (valueAttribute.Update, this, false);
            }

            CustomUpdateEventAvailable = _addUpdateDelegate != null || _addNotifyDelegate != null;
        }


        internal bool TrySubscribeToUpdateEvent(TTarget target, Action refreshAction, Action<TValue> setValueDelegate)
        {
            if (_addUpdateDelegate != null)
            {
                _addUpdateDelegate(target, setValueDelegate);
                return true;
            }

            if (_addNotifyDelegate == null) return false;
            _addNotifyDelegate(target, refreshAction);
            return true;
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

        #region --- [REFLECTION FIELDS] ---

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

        #region --- [CUSTOM UPDATE EVENT] ---

        private delegate void UpdateHandleDelegate<in T, out TParam>(T target, Action<TParam> listener);

        private delegate void NotifyHandleDelegate<in T>(T target, Action listener);

        private static UpdateHandleDelegate<T, TParam> CreateUpdateHandlerDelegate<T, TParam>(
            string eventName, MonitorProfile profile, bool createAddMethod)
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
                if (action != null) return action;
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
                if (action != null) return (target, value) => action(value);
            }

            return null;
        }

        //--------------------------------------------------------------------------------------------------------------        

        private static NotifyHandleDelegate<T> CreateNotifyHandlerDelegate<T>(string eventName,
            MonitorProfile profile, bool createAddMethod)
        {
            // check instance events:
            var instanceEvent = profile.UnitTargetType.GetEvent(eventName, INSTANCE_FLAGS);
            if (instanceEvent != null)
            {
                var method = createAddMethod
                    ? instanceEvent.GetAddMethod(true)
                    : instanceEvent.GetRemoveMethod(true);

                var action =
                    (NotifyHandleDelegate<T>) Delegate.CreateDelegate(typeof(NotifyHandleDelegate<T>),
                        method, false);
                if (action != null) return action;
            }


            //------------------------
            var staticEvent = profile.UnitTargetType.GetEvent(eventName, STATIC_FLAGS);
            if (staticEvent != null)
            {
                var method = createAddMethod
                    ? staticEvent.GetAddMethod(true)
                    : staticEvent.GetRemoveMethod(true);

                var action = (Action<Action>) Delegate.CreateDelegate(typeof(Action<Action>), method, false);
                if (action != null) return (target, value) => action(value);
            }

            return null;
        }

        #endregion
    }
}
// Copyright (c) 2022 Jonathan Lang

using Baracuda.Monitoring.Profiles;
using System;
using System.Runtime.CompilerServices;

namespace Baracuda.Monitoring.Units
{
    internal class EventHandle<TTarget, TDelegate> : MonitorHandle where TTarget : class where TDelegate : Delegate
    {
        private readonly EventProfile<TTarget, TDelegate>.StateFormatDelegate _stateFormatter;
        private readonly EventProfile<TTarget, TDelegate> _eventProfile;
        private readonly TTarget _target;

        private readonly Delegate _eventHandler;
        private int _invokeCounter;

        //--------------------------------------------------------------------------------------------------------------

        internal EventHandle(
            TTarget target,
            EventProfile<TTarget, TDelegate>.StateFormatDelegate stateFormatter,
            EventProfile<TTarget, TDelegate> eventProfile) : base(target, eventProfile)
        {
            _target = target;
            _eventProfile = eventProfile;
            _stateFormatter = stateFormatter;
            _eventHandler = eventProfile.CreateMatchingDelegate(OnEvent);
            eventProfile.SubscribeToEvent(target, _eventHandler);
        }

        //--------------------------------------------------------------------------------------------------------------

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override string GetState()
        {
            return _stateFormatter(_target, _invokeCounter);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override object GetValueAsObject()
        {
            return _eventHandler;
        }

        public override void Refresh()
        {
            var state = GetState();
            RaiseValueChanged(state);
        }

        private void OnEvent()
        {
            _invokeCounter++;
            var state = GetState();
            RaiseValueChanged(state);
        }

        public override void Dispose()
        {
            base.Dispose();
            _eventProfile.UnsubscribeFromEvent(_target, _eventHandler);
        }
    }
}
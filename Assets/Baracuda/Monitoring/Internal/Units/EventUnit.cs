// Copyright (c) 2022 Jonathan Lang
using System;
using System.Runtime.CompilerServices;
using Baracuda.Monitoring.Interface;
using Baracuda.Monitoring.Internal.Profiling;

namespace Baracuda.Monitoring.Internal.Units
{
    public class EventUnit<TTarget, TDelegate> : MonitorUnit where TTarget : class where TDelegate : Delegate
    {
        #region --- Properties ---

        public override IMonitorProfile Profile => _eventProfile;

        #endregion
        
        //--------------------------------------------------------------------------------------------------------------
        
        #region --- Fields ---

        private readonly EventProfile<TTarget, TDelegate>.StateFormatDelegate _stateFormatter;
        private readonly EventProfile<TTarget, TDelegate> _eventProfile;
        private readonly TTarget _target;

        private readonly Delegate _eventHandler;
        private int _invokeCounter = 0;

        #endregion
        
        //--------------------------------------------------------------------------------------------------------------

        internal EventUnit(
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
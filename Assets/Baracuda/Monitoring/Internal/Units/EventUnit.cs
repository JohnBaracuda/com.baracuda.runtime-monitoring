// Copyright (c) 2022 Jonathan Lang (CC BY-NC-SA 4.0)
using System;
using Baracuda.Monitoring.Interface;
using Baracuda.Monitoring.Internal.Profiling;

namespace Baracuda.Monitoring.Internal.Units
{
    public class EventUnit<TTarget, TDelegate> : MonitorUnit where TTarget : class where TDelegate : Delegate
    {
        #region --- Properties ---

        public override string GetStateFormatted => _stateFormatter(_target, _invokeCounter);
        public override string GetStateRaw { get; } = "INVALID";
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
            eventProfile.SubscribeEventHandler(target, _eventHandler);
            ExternalUpdateRequired = eventProfile.Refresh;
        }
        
        //--------------------------------------------------------------------------------------------------------------
     
        public override void Refresh()
        {
            RaiseValueChanged(GetStateFormatted);
        }

        private void OnEvent()
        {
            _invokeCounter++;
            RaiseValueChanged(GetStateFormatted);
        }

        public override void Dispose()
        {
            base.Dispose();
            _eventProfile.RemoveEventHandler(_target, _eventHandler);
        }
    }
}
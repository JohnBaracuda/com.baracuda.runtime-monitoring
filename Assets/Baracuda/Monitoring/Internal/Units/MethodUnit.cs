// Copyright (c) 2022 Jonathan Lang

using System;
using Baracuda.Monitoring.Interface;
using Baracuda.Monitoring.Internal.Profiling;

namespace Baracuda.Monitoring.Internal.Units
{
    public sealed class MethodUnit<TTarget, TValue> : ValueUnit<TTarget, TValue> where TTarget : class
    {
        #region --- Properties ---

        public override IMonitorProfile Profile => _methodProfile;

        #endregion
        
        //--------------------------------------------------------------------------------------------------------------
        
        #region --- Fields ---

        private readonly MethodProfile<TTarget, TValue> _methodProfile;

        #endregion
        
        //--------------------------------------------------------------------------------------------------------------
        
        public MethodUnit(
            TTarget target, 
            Func<TTarget, TValue> getValue,
            Action<TTarget, TValue> setValue,
            Func<TValue, string> valueProcessor,
            MethodProfile<TTarget, TValue> profile) : base(target, getValue, setValue, valueProcessor, profile)
        {
            _methodProfile = profile;
        }
    }
}
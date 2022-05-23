// Copyright (c) 2022 Jonathan Lang
using System;
using Baracuda.Monitoring.Interface;
using Baracuda.Monitoring.Internal.Profiling;

namespace Baracuda.Monitoring.Internal.Units
{
    public sealed class FieldUnit<TTarget, TValue> : ValueUnit<TTarget, TValue> where TTarget : class
    {
        #region --- Properties ---

        public override IMonitorProfile Profile => _fieldProfile;

        #endregion
        
        //--------------------------------------------------------------------------------------------------------------
        
        #region --- Fields ---

        private readonly FieldProfile<TTarget, TValue> _fieldProfile;
        
        #endregion
        
        //--------------------------------------------------------------------------------------------------------------

        #region --- Ctr ---

        internal FieldUnit(
            TTarget target,
            Func<TTarget, TValue> getValue,
            Action<TTarget, TValue> setValue,
            Func<TValue, string> valueProcessor,
            FieldProfile<TTarget, TValue> fieldProfile) 
            : base(target, getValue, setValue, valueProcessor, fieldProfile)
        {
            _fieldProfile = fieldProfile;
        }
       
        #endregion

    }
}
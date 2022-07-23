// Copyright (c) 2022 Jonathan Lang

using System;
using Baracuda.Monitoring.Core.Profiling;
using Baracuda.Monitoring.Interface;

namespace Baracuda.Monitoring.Core.Units
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
            MulticastDelegate validator,
            FieldProfile<TTarget, TValue> fieldProfile) 
            : base(target, getValue, setValue, valueProcessor, validator, fieldProfile)
        {
            _fieldProfile = fieldProfile;
        }


        #endregion

    }
}
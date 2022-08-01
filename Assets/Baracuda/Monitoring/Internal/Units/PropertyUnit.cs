// Copyright (c) 2022 Jonathan Lang
using System;
using Baracuda.Monitoring.Interface;
using Baracuda.Monitoring.Internal.Profiling;

namespace Baracuda.Monitoring.Internal.Units
{
    public sealed class PropertyUnit<TTarget, TValue> : ValueUnit<TTarget, TValue> where TTarget : class
    {
        
        #region --- Properties ---

        public override IMonitorProfile Profile => _propertyProfile;

        #endregion
        
        //--------------------------------------------------------------------------------------------------------------
        
        #region --- Fields ---

        private readonly PropertyProfile<TTarget, TValue> _propertyProfile;

        #endregion
        
        //--------------------------------------------------------------------------------------------------------------

        #region --- Ctr ---

        internal PropertyUnit(TTarget target, 
            Func<TTarget, TValue> getValue, 
            Action<TTarget, TValue> setValue, 
            Func<TValue, string> valueProcessor,
            PropertyProfile<TTarget, TValue> propertyProfile) 
            : base (target, getValue, setValue, valueProcessor, propertyProfile)
        {
            _propertyProfile = propertyProfile;
        }
       

        #endregion
      
    }
}
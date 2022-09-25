// Copyright (c) 2022 Jonathan Lang

using Baracuda.Monitoring.Profiles;
using Baracuda.Monitoring.Types;
using System;

namespace Baracuda.Monitoring.Units
{
    public sealed class PropertyUnit<TTarget, TValue> : ValueUnit<TTarget, TValue> where TTarget : class
    {
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
            MulticastDelegate validationFunc,
            ValidationEvent validationEvent,
            PropertyProfile<TTarget, TValue> propertyProfile) 
            : base (target, getValue, setValue, valueProcessor, validationFunc, validationEvent, propertyProfile)
        {
            _propertyProfile = propertyProfile;
        }
       

        #endregion
    }
}
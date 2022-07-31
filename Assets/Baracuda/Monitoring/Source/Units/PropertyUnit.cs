// Copyright (c) 2022 Jonathan Lang

using System;
using Baracuda.Monitoring.API;
using Baracuda.Monitoring.Source.Profiles;
using Baracuda.Monitoring.Source.Types;

namespace Baracuda.Monitoring.Source.Units
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
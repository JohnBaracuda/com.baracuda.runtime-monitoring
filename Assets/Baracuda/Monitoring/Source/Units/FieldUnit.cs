// Copyright (c) 2022 Jonathan Lang

using System;
using Baracuda.Monitoring.API;
using Baracuda.Monitoring.Source.Profiles;
using Baracuda.Monitoring.Source.Types;

namespace Baracuda.Monitoring.Source.Units
{
    public sealed class FieldUnit<TTarget, TValue> : ValueUnit<TTarget, TValue> where TTarget : class
    {
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
            MulticastDelegate validationFunc,
            ValidationEvent validationEvent,
            FieldProfile<TTarget, TValue> fieldProfile) 
            : base(target, getValue, setValue, valueProcessor, validationFunc, validationEvent, fieldProfile)
        {
            _fieldProfile = fieldProfile;
        }


        #endregion

    }
}
// Copyright (c) 2022 Jonathan Lang

using Baracuda.Monitoring.Core.Profiles;
using Baracuda.Monitoring.Core.Types;
using System;

namespace Baracuda.Monitoring.Core.Units
{
    internal sealed class FieldUnit<TTarget, TValue> : ValueUnit<TTarget, TValue> where TTarget : class
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
// Copyright (c) 2022 Jonathan Lang

using Baracuda.Monitoring.Profiles;
using Baracuda.Monitoring.Types;
using System;

namespace Baracuda.Monitoring.Units
{
    internal sealed class FieldHandle<TTarget, TValue> : ValueHandle<TTarget, TValue> where TTarget : class
    {
        //--------------------------------------------------------------------------------------------------------------

        #region --- Fields ---

        private readonly FieldProfile<TTarget, TValue> _fieldProfile;

        #endregion

        //--------------------------------------------------------------------------------------------------------------

        #region --- Ctr ---

        internal FieldHandle(
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
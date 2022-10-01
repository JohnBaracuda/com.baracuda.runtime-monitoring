// Copyright (c) 2022 Jonathan Lang

using Baracuda.Monitoring.Core.Profiles;
using Baracuda.Monitoring.Core.Types;
using System;

namespace Baracuda.Monitoring.Core.Units
{
    internal sealed class PropertyUnit<TTarget, TValue> : ValueUnit<TTarget, TValue> where TTarget : class
    {
        internal PropertyUnit(TTarget target,
            Func<TTarget, TValue> getValue,
            Action<TTarget, TValue> setValue,
            Func<TValue, string> valueProcessor,
            MulticastDelegate validationFunc,
            ValidationEvent validationEvent,
            PropertyProfile<TTarget, TValue> propertyProfile)
            : base(target, getValue, setValue, valueProcessor, validationFunc, validationEvent, propertyProfile)
        {
        }
    }
}
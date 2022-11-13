// Copyright (c) 2022 Jonathan Lang

using System;

namespace Baracuda.Monitoring
{
    [Obsolete("This API will be removed in 4.0.0")]
    public interface ISettableValue<in TValue> : ISettableValue
    {
        [Obsolete("This API will be removed in 4.0.0")]
        void SetValue(TValue value);
    }

    [Obsolete("This API will be removed in 4.0.0")]
    public interface ISettableValue
    {

        [Obsolete("This API will be removed in 4.0.0")]
        void SetValue(object value);

        [Obsolete("This API will be removed in 4.0.0")]
        void SetValueStruct<TStruct>(TStruct value) where TStruct : struct;
    }
}
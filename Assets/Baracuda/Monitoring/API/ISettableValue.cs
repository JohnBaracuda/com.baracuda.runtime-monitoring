// Copyright (c) 2022 Jonathan Lang

namespace Baracuda.Monitoring.API
{
    public interface ISettableValue<in TValue> : ISettableValue
    {
        void SetValue(TValue value);
    }
    
    public interface ISettableValue
    {
        void SetValue(object value);
        void SetValueStruct<TStruct>(TStruct value) where TStruct : struct;
    }
}
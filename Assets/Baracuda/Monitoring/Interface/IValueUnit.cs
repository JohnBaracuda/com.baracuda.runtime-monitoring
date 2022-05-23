// Copyright (c) 2022 Jonathan Lang
namespace Baracuda.Monitoring.Interface
{
    public interface IValueUnit
    {
        TValue GetValue<TValue>();
        void SetValue(object value);
        void SetValue<T>(T value) where T : unmanaged;
    }
}

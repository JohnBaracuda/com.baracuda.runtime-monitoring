// Copyright (c) 2022 Jonathan Lang
namespace Baracuda.Monitoring.Interface
{
    public interface IValueUnit<out TValue> : IValueUnit
    {
        TValue GetValue();
    }
    
    public interface IValueUnit
    {
        TValue GetValueConverted<TValue>();

        object GetValueAsObject();
        
        void SetValue(object value);
        
        void SetValue<T>(T value) where T : unmanaged;
    }
}

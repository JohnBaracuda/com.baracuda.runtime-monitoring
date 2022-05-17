// Copyright (c) 2022 Jonathan Lang (CC BY-NC-SA 4.0)
namespace Baracuda.Monitoring.Interface
{
    public interface IValueUnit
    {
        TValue GetValue<TValue>();
        
        void SetValue(object value);
#if UNITY_2020_1_OR_NEWER
        void SetValue<T>(T value) where T : unmanaged;
#endif
    }
}

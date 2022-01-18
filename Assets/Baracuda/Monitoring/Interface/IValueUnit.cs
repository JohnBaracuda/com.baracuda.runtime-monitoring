namespace Baracuda.Monitoring.Interface
{
    public interface IValueUnit
    {
        void SetValue(object value);
#if UNITY_2020_1_OR_NEWER
        void SetValue<T>(T value) where T : unmanaged;
#endif
    }
}

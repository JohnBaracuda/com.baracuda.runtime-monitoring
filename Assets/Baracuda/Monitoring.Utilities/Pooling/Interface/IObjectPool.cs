using Baracuda.Monitoring.Utilities.Pooling.Utils;

namespace Baracuda.Monitoring.Utilities.Pooling.Interface
{
    public interface IObjectPool<T>
    {
        int CountInactive { get; }
        T Get();
        PooledObject<T> GetDisposable();
        void Release(T element);
        void Clear();
    }
}
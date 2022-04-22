using Baracuda.Monitoring.Internal.Pooling.Utils;

namespace Baracuda.Monitoring.Internal.Pooling.Interface
{
    internal interface IObjectPool<T>
    {
        int CountInactive { get; }
        T Get();
        PooledObject<T> GetDisposable();
        void Release(T element);
        void Clear();
    }
}
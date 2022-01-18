using Baracuda.Pooling.Utils;

namespace Baracuda.Pooling.Interfaces
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
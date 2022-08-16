// Copyright (c) 2022 Jonathan Lang

namespace Baracuda.Utilities.Pooling
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
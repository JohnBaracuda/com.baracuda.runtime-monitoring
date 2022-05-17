// Copyright (c) 2022 Jonathan Lang (CC BY-NC-SA 4.0)
using Baracuda.Pooling.Utils;

namespace Baracuda.Pooling.Interface
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
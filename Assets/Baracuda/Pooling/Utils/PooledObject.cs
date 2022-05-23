// Copyright (c) 2022 Jonathan Lang
using System;
using Baracuda.Pooling.Interface;

namespace Baracuda.Pooling.Utils
{
    public readonly struct PooledObject<T> : IDisposable
    {
        public readonly T Value;
        private readonly IObjectPool<T> _pool;

        internal PooledObject(T value, IObjectPool<T> pool)
        {
            Value = value;
            _pool = pool;
        }

        void IDisposable.Dispose()
        {
            _pool.Release(Value);
        }
        
        public static implicit operator T (PooledObject<T> pooledObject)
        {
            return pooledObject.Value;
        }

        public override string ToString()
        {
            return Value.ToString();
        }

        public static implicit operator string(PooledObject<T> current)
        {
            return current.ToString();
        }
    }
}
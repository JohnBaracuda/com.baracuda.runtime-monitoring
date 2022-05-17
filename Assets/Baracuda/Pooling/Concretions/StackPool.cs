// Copyright (c) 2022 Jonathan Lang (CC BY-NC-SA 4.0)
using System.Collections.Generic;
using Baracuda.Pooling.Abstractions;
using Baracuda.Pooling.Utils;

namespace Baracuda.Pooling.Concretions
{
    public class StackPool<T>
    {
        private static readonly ObjectPoolT<Stack<T>> pool 
            = new ObjectPoolT<Stack<T>>(() => new Stack<T>(), actionOnRelease: l => l.Clear());

        public static Stack<T> Get()
        {
            return pool.Get();
        }
        
        public static void Release(Stack<T> toRelease)
        {
            pool.Release(toRelease);
        }

        public static PooledObject<Stack<T>> GetDisposable()
        {
            return pool.GetDisposable();
        }
    }
}

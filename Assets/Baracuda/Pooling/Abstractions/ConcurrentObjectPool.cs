// Copyright (c) 2022 Jonathan Lang
using System;
using JetBrains.Annotations;

namespace Baracuda.Pooling.Abstractions
{
    public class ConcurrentObjectPool<T> : ObjectPool<T>
    {
        public ConcurrentObjectPool(
            [NotNull] Func<T> createFunc,
            Action<T> actionOnGet = null,
            Action<T> actionOnRelease = null,
            Action<T> actionOnDestroy = null,
            bool collectionCheck = true,
            int defaultCapacity = 1,
            int maxSize = 10000) :
            base(createFunc, actionOnGet, actionOnRelease, actionOnDestroy, collectionCheck, defaultCapacity, maxSize)
        {
        }
        
        public override T Get()
        {
            T obj;
            lock (Stack)
            {
                if (Stack.Count == 0)
                {
                    obj = CreateFunc();
                    ++CountAll;
                }
                else
                {
                    obj = Stack.Pop();
                }
            }
            ActionOnGet?.Invoke(obj);
#if MONITOR_POOLS
            m_accessed++;
#endif
            return obj;
        }

        public override void Release(T element)
        {
            lock (Stack)
            {
                if (CollectionCheck && Stack.Count > 0 && Stack.Contains(element))
                {
                    throw new InvalidOperationException("Trying to release an object that has already been released to the pool.");
                }

                ActionOnRelease?.Invoke(element);
                if (CountInactive < MaxSize)
                {
                    Stack.Push(element);
                }
                else
                {
                    ActionOnDestroy?.Invoke(element);
                }
            }
        }

        public override void Clear()
        {
            lock (Stack)
            {
                if (ActionOnDestroy != null)
                {
                    foreach (var obj in Stack)
                    {
                        ActionOnDestroy(obj);
                    }
                }

                Stack.Clear();
                CountAll = 0;
            }
        }
    }
}
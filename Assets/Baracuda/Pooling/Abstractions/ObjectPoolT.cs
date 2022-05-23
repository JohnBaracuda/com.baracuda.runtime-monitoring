// Copyright (c) 2022 Jonathan Lang
using System;
using JetBrains.Annotations;

namespace Baracuda.Pooling.Abstractions
{
    public class ObjectPoolT<T> : ObjectPool<T>
    {
        /*
         *  Ctor   
         */

        public ObjectPoolT(
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

        /*
         *  Implementations   
         */

        public override T Get()
        {
#if ENFORCETHREADSAFETY
            if (MainThread.ManagedThreadId != System.Threading.Thread.CurrentThread.ManagedThreadId)
            {
                throw new InvalidOperationException(
                    $"{typeof(ObjectPoolT<T>).Name}.{nameof(Get)} is only allowed to be called from the main thread! Use " +
                    $"{typeof(ConcurrentObjectPool<>).Name} when accessing objet pools from multiple threads!");
            } 
#endif
            T obj;
            if (Stack.Count == 0)
            {
                obj = CreateFunc();
                ++CountAll;
            }
            else
            {
                obj = Stack.Pop();
            }

            ActionOnGet?.Invoke(obj);
            return obj;
        }

        public override void Release(T element)
        {
#if ENFORCETHREADSAFETY
            if (MainThread.ManagedThreadId != System.Threading.Thread.CurrentThread.ManagedThreadId)
            {
                throw new InvalidOperationException(
                    $"{typeof(ConcurrentObjectPool<T>).Name}.{nameof(Release)} is only allowed to be called from the main thread! Use " +
                    $"{typeof(ConcurrentObjectPool<>).Name} when accessing objet pools from multiple threads!");
            } 
#endif
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

        public override void Clear()
        {
#if ENFORCETHREADSAFETY
            if (MainThread.ManagedThreadId != System.Threading.Thread.CurrentThread.ManagedThreadId)
            {
                throw new InvalidOperationException(
                    $"{typeof(ConcurrentObjectPool<T>).Name}.{nameof(Clear)} is only allowed to be called from the main thread! Use " +
                    $"{typeof(ConcurrentObjectPool<>).Name} when accessing objet pools from multiple threads!");
            } 
#endif
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
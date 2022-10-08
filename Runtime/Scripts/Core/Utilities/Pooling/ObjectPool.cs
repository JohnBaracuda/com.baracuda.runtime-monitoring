// Copyright (c) 2022 Jonathan Lang

using JetBrains.Annotations;
using System;
using System.Collections.Generic;

namespace Baracuda.Monitoring.Utilities.Pooling
{
    internal class ObjectPool<T>
    {
        /*
         *  Properties
         */

        public int CountAll { get; private protected set; }
        public int CountInactive => Stack.Count;

        protected Stack<T> Stack { get; }
        protected Func<T> CreateFunc { get; }
        protected Action<T> ActionOnGet { get; }
        protected Action<T> ActionOnRelease { get; }
        protected Action<T> ActionOnDestroy { get; }
        protected int MaxSize { get; }
        protected bool CollectionCheck { get; }

        /*
         *  Ctor
         */

        public ObjectPool(
            [NotNull] Func<T> createFunc,
            Action<T> actionOnGet = null,
            Action<T> actionOnRelease = null,
            Action<T> actionOnDestroy = null,
            bool collectionCheck = true,
            int defaultCapacity = 1,
            int maxSize = 10000)
        {
            if (maxSize <= 0)
            {
                throw new ArgumentException("Max Size must be greater than 0", nameof(maxSize));
            }

            Stack = new Stack<T>(defaultCapacity);

            CreateFunc = createFunc ?? throw new ArgumentNullException(nameof(createFunc));
            MaxSize = maxSize;
            ActionOnGet = actionOnGet;
            ActionOnRelease = actionOnRelease;
            ActionOnDestroy = actionOnDestroy;
            CollectionCheck = collectionCheck;

            for (var i = 0; i < defaultCapacity; i++)
            {
                Stack.Push(CreateFunc());
                ++CountAll;
            }
        }

        public virtual T Get()
        {
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

        public virtual void Release(T element)
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
}
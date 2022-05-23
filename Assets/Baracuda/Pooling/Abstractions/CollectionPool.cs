// Copyright (c) 2022 Jonathan Lang
using System.Collections.Generic;
using Baracuda.Pooling.Utils;

namespace Baracuda.Pooling.Abstractions
{
    public class CollectionPool<TCollection, TItem> where TCollection : class, ICollection<TItem>, new()
    {
        public static int CountAll => pool.CountAll;
        
        private static readonly ObjectPoolT<TCollection> pool 
            = new ObjectPoolT<TCollection>(() => new TCollection(), actionOnRelease: l => l.Clear());

        /// <summary>
        /// Get an object from the pool. Must be manually released back to the pool by calling Release.
        /// This operation is not thread safe!
        /// </summary>
        /// <returns></returns>
        public static TCollection Get()
        {
            return pool.Get();
        }

        /// <summary>
        /// Release an object to the pool.
        /// This operation is not thread safe!
        /// </summary>
        public static void Release(TCollection toRelease)
        {
            pool.Release(toRelease);
        }
        
        /// <summary>
        /// Get a disposable object to the pool that will automatically be released back to the pool.
        /// Access the pooled object by the <see cref="PooledObject{T}.Value"/> property. 
        /// </summary>
        public static PooledObject<TCollection> GetDisposable()
        {
            return pool.GetDisposable();
        }
    }
}
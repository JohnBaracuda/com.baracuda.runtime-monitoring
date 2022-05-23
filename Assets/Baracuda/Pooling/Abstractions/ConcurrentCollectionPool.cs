// Copyright (c) 2022 Jonathan Lang
using System.Collections.Generic;
using Baracuda.Pooling.Utils;

namespace Baracuda.Pooling.Abstractions
{
    public class ConcurrentCollectionPool<TCollection, TItem> where TCollection : class, ICollection<TItem>, new()
    {
        private static readonly ConcurrentObjectPool<TCollection> pool 
            = new ConcurrentObjectPool<TCollection>(() => new TCollection(), actionOnRelease: l => l.Clear());

        /// <summary>
        /// This operation is thread safe!
        /// Get an object from the pool. Must be manually released back to the pool by calling Release.
        /// </summary>
        public static TCollection Get()
        {
            return pool.Get();
        }

        /// <summary>
        /// This operation is thread safe!
        /// Release an object to the pool. opti in a thread safe manner. 
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
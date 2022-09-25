// Copyright (c) 2022 Jonathan Lang

using System.Collections.Generic;

namespace Baracuda.Monitoring.Utilities.Pooling
{
    internal class ConcurrentCollectionPool<TCollection, TItem> where TCollection : class, ICollection<TItem>, new()
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
    }
}
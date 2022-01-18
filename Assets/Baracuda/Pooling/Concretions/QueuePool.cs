using System.Collections.Generic;
using Baracuda.Pooling.Abstractions;
using Baracuda.Pooling.Utils;

namespace Baracuda.Pooling.Concretions
{
#if JETBRAINS_ANNOTATIONS
    [JetBrains.Annotations.UsedImplicitly]
#endif
    public class QueuePool<T>
    {
        private static readonly ObjectPoolT<Queue<T>> _poolTBase 
            = new ObjectPoolT<Queue<T>>(() => new Queue<T>(), actionOnRelease: l => l.Clear());

        public static Queue<T> Get()
        {
            return _poolTBase.Get();
        }
        
        public static void Release(Queue<T> toRelease)
        {
            _poolTBase.Release(toRelease);
        }
        
        public static PooledObject<Queue<T>> GetDisposable()
        {
            return _poolTBase.GetDisposable();
        }
    }
}
using System.Collections.Generic;
using Baracuda.Pooling.Abstractions;
using Baracuda.Pooling.Utils;

namespace Baracuda.Pooling.Concretions
{
#if JETBRAINS_ANNOTATIONS
    [JetBrains.Annotations.UsedImplicitly]
#endif
    public class StackPool<T>
    {
        private static readonly ObjectPoolT<Stack<T>> _pool 
            = new ObjectPoolT<Stack<T>>(() => new Stack<T>(), actionOnRelease: l => l.Clear());

        public static Stack<T> Get()
        {
            return _pool.Get();
        }
        
        public static void Release(Stack<T> toRelease)
        {
            _pool.Release(toRelease);
        }

        public static PooledObject<Stack<T>> GetDisposable()
        {
            return _pool.GetDisposable();
        }
    }
}
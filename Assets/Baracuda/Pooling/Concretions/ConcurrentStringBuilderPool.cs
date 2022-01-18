using System.Text;
using Baracuda.Pooling.Abstractions;
using Baracuda.Pooling.Utils;
using UnityEngine;

namespace Baracuda.Pooling.Concretions
{
    /// <summary>
    /// Thread safe version of a <see cref="StringBuilderPool"/>
    /// </summary>
    public static class ConcurrentStringBuilderPool
    {
        // Hack used to guarantee the initialization of the StringBuilder Pool
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Init()
        {
        }
        
        private static readonly ConcurrentObjectPool<StringBuilder> _pool = 
            new ConcurrentObjectPool<StringBuilder>(() => new StringBuilder(100), actionOnRelease: builder => builder.Clear());

        public static StringBuilder Get()
        {
            return _pool.Get();
        }
        
        public static void ReleaseStringBuilder(StringBuilder toRelease)
        {
            _pool.Release(toRelease);
        }
        
        public static string Release(StringBuilder toRelease)
        {
            var str = toRelease.ToString();
            _pool.Release(toRelease);
            return str;
        }
        
        public static PooledObject<StringBuilder> GetDisposable()
        {
            return _pool.GetDisposable();
        }
    }
}
// Copyright (c) 2022 Jonathan Lang
using System.Text;
using Baracuda.Pooling.Abstractions;
using Baracuda.Pooling.Utils;
using UnityEngine;

namespace Baracuda.Pooling.Concretions
{
    public static class StringBuilderPool
    {
        // Hack used to guarantee the initialization of the StringBuilder Pool
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Init()
        {
        }
        
        private static readonly ObjectPoolT<StringBuilder> pool = 
            new ObjectPoolT<StringBuilder>(() => new StringBuilder(100), actionOnRelease: builder => builder.Clear());
        
        public static StringBuilder Get()
        {
            return pool.Get();
        }
        
        public static void ReleaseStringBuilder(StringBuilder toRelease)
        {
            pool.Release(toRelease);
        }
        
        public static string Release(StringBuilder toRelease)
        {
            var str = toRelease.ToString();
            pool.Release(toRelease);
            return str;
        }
        
        public static PooledObject<StringBuilder> GetDisposable()
        {
            return pool.GetDisposable();
        }
    }
}
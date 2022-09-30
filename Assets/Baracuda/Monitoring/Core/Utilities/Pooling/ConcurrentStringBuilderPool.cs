// Copyright (c) 2022 Jonathan Lang

using System.Text;
using UnityEngine;

namespace Baracuda.Monitoring.Utilities.Pooling
{
    /// <summary>
    /// Thread safe version of a <see cref="StringBuilderPool"/>
    /// </summary>
    internal static class ConcurrentStringBuilderPool
    {
        // Hack used to guarantee the initialization of the StringBuilder Pool
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Init()
        {
        }

        private static readonly ConcurrentObjectPool<StringBuilder> pool =
            new ConcurrentObjectPool<StringBuilder>(() => new StringBuilder(100), actionOnRelease: builder => builder.Clear());

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
    }
}
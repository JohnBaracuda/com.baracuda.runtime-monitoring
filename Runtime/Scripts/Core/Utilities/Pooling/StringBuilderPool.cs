// Copyright (c) 2022 Jonathan Lang

using System.Text;
using UnityEngine;

namespace Baracuda.Monitoring.Utilities.Pooling
{
    internal static class StringBuilderPool
    {
        // Hack used to guarantee the initialization of the StringBuilder Pool
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Init()
        {
        }

        private static readonly ObjectPool<StringBuilder> pool =
            new ObjectPool<StringBuilder>(() => new StringBuilder(100), actionOnRelease: builder => builder.Clear());

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
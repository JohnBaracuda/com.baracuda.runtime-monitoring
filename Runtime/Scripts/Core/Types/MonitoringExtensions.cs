// Copyright (c) 2022 Jonathan Lang

using Baracuda.Monitoring.Utilities.Pooling;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
#if UNITY_2020_1_OR_NEWER
#endif

namespace Baracuda.Monitoring.Types
{
    internal static class MonitoringExtensions
    {
        /// <summary>
        /// Converts the target to be of the specified type.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static TTo ConvertFast<TFrom, TTo>(this TFrom value)
        {
#if UNITY_2020_1_OR_NEWER
            return UnsafeUtility.As<TFrom, TTo>(ref value);

#else
            return (TTo)(object)value;
#endif
        }

        /// <summary>
        /// Try to convert the target to the specified type.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static bool TryConvert<TFrom, TTo>(this TFrom value, out TTo result)
        {
            try
            {
                result = (TTo) Convert.ChangeType(value, typeof(TTo));
                return true;
            }
            catch (Exception)
            {
                result = default;
                return false;
            }
        }

        /*
         * Enum Flags
         */

        internal static bool HasFlagFast(this int lhs, int rhs)
        {
            return unchecked((uint) lhs & (uint) rhs) > 0;
        }

        /*
         * Color
         */

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static string ColorizeString(this string content, Color color)
        {
            var sb = ConcurrentStringBuilderPool.Get();
            sb.Append("<color=#");
            sb.Append(ColorUtility.ToHtmlStringRGB(color));
            sb.Append('>');
            sb.Append(content);
            sb.Append("</color>");
            return ConcurrentStringBuilderPool.Release(sb);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static bool AddUnique<T>(this IList<T> list, T item)
        {
            if (list.Contains(item))
            {
                return false;
            }

            list.Add(item);
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static bool TryGetValueValidated<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, out TValue value) where TValue : UnityEngine.Object
        {
            if (!dictionary.TryGetValue(key, out value))
            {
                return false;
            }

            if (value != null)
            {
                return true;
            }

            dictionary.Remove(key);
            return false;
        }
    }
}

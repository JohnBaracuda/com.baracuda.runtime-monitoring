// Copyright (c) 2022 Jonathan Lang

using System;
using System.Runtime.CompilerServices;
using Baracuda.Utilities.Pooling;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
#if UNITY_2020_1_OR_NEWER
#endif

namespace Baracuda.Monitoring.Source.Types
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
                result = (TTo)Convert.ChangeType(value, typeof(TTo));
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
            return unchecked ((uint)lhs & (uint)rhs) > 0; 
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
    }
}

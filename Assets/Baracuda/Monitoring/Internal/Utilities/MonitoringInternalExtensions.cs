// Copyright (c) 2022 Jonathan Lang
using System;
using System.Runtime.CompilerServices;
using Baracuda.Pooling.Concretions;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

namespace Baracuda.Monitoring.Internal.Utilities
{
    public static class MonitoringInternalExtensions
    {
        /*
         *  Type conversion   
         */
        
        /// <summary>
        /// Converts the target to be of the specified type.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T2 ConvertFast<T1, T2>(this T1 value)
        {
#if UNITY_2020_1_OR_NEWER
            return UnsafeUtility.As<T1, T2>(ref value);
#else
            return (T2)(object)value;
#endif
        }

        /*
         * Enum Flags   
         */
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool HasFlagUnsafe<TEnum>(this TEnum lhs, TEnum rhs) where TEnum : unmanaged, Enum
        {
#if UNITY_2020_1_OR_NEWER
            switch (UnsafeUtility.SizeOf<TEnum>())
            {
                case 1:
                    return (UnsafeUtility.As<TEnum, byte>(ref lhs) & UnsafeUtility.As<TEnum, byte>(ref rhs)) > 0;
                case 2:
                    return (UnsafeUtility.As<TEnum, ushort>(ref lhs) & UnsafeUtility.As<TEnum, ushort>(ref rhs)) > 0;
                case 4:
                    return (UnsafeUtility.As<TEnum, uint>(ref lhs) & UnsafeUtility.As<TEnum, uint>(ref rhs)) > 0;
                case 8:
                    return (UnsafeUtility.As<TEnum, ulong>(ref lhs) & UnsafeUtility.As<TEnum, ulong>(ref rhs)) > 0;
                default:
                    throw new Exception($"Size of {typeof(TEnum).Name} does not match a known Enum backing type.");
            }
#else
            return lhs.HasFlag(rhs);
#endif
        }

        public static bool HasFlag32(this int lhs, int rhs)
        {
            return ((uint)lhs & (uint)rhs) > 0; 
        }

        /*
         * Color
         */

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string Colorize(this string content, Color color)
        {
            var pooled = ConcurrentStringBuilderPool.Get();
            pooled.Append("<color=#");
            pooled.Append(ColorUtility.ToHtmlStringRGBA(color));
            pooled.Append('>');
            pooled.Append(content);
            pooled.Append("</color>");
            return ConcurrentStringBuilderPool.Release(pooled);
        }

        /*
         * Text Manipulation   
         */
        
        public static string Humanize(this string target, string[] prefixes = null)
        {
            if (IsConstantStringSyntax(target))
            {
                return target.Replace('_', ' ').ToCamel();
            }
            
            if (prefixes != null)
            {
                for (var i = 0; i < prefixes.Length; i++)
                {
                    if (target.StartsWith(prefixes[i]))
                    {
                        target = target.Replace(prefixes[i], string.Empty);
                    }
                }
            }
            
            target = target.Replace('_', ' ');
            
            var chars = ConcurrentListPool<char>.Get();
            
            for (var i = 0; i < target.Length; i++)
            {
                if (i == 0)
                {
                    chars.Add(char.ToUpper(target[i]));
                    continue;
                }

                if (i < target.Length - 1)
                {
                    if (char.IsUpper(target[i]) && !char.IsUpper(target[i + 1])
                        || char.IsUpper(target[i]) && !char.IsUpper(target[i - 1]))
                    {
                        if (i > 1)
                        {
                            chars.Add(' ');
                        }
                    }
                }

                chars.Add(target[i]);
            }

            var array = chars.ToArray();
            ConcurrentListPool<char>.Release(chars);
            return new string(array).ReduceWhitespace();
        }

        public static bool BeginsWith(this string str, char character)
        {
            return !string.IsNullOrWhiteSpace(str) && str[0] == character;
        }
        
        
        private static string ToCamel(this string content)
        {
            var sb = ConcurrentStringBuilderPool.Get();

            for (var i = 0; i < content.Length; i++)
            {
                var current = content[i];
                var last = i > 0 ? content[i - 1] : ' ';
                sb.Append(last == ' ' ? char.ToUpper(current) : char.ToLower(current));
            }

            return ConcurrentStringBuilderPool.Release(sb);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool IsConstantStringSyntax(this string input)
        {
            for (var i = 0; i < input.Length; i++)
            {
                var character = input[i];
                if (!char.IsUpper(character) && character != '_')
                {
                    return false;
                }
            }

            return true;
        }


        private static string ReduceWhitespace(this string value)
        {
            var sb = ConcurrentStringBuilderPool.Get();
            var previousIsWhitespaceFlag = false;
            for (var i = 0; i < value.Length; i++)
            {
                if (char.IsWhiteSpace(value[i]))
                {
                    if (previousIsWhitespaceFlag)
                    {
                        continue;
                    }

                    previousIsWhitespaceFlag = true;
                }
                else
                {
                    previousIsWhitespaceFlag = false;
                }

                sb.Append(value[i]);
            }

            return ConcurrentStringBuilderPool.Release(sb);
        }
    }
}

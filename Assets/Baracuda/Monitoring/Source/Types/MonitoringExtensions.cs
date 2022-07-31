// Copyright (c) 2022 Jonathan Lang

using System;
using System.Runtime.CompilerServices;
using Baracuda.Pooling.Concretions;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
#if UNITY_2020_1_OR_NEWER
#endif

namespace Baracuda.Monitoring.Source.Types
{
    public static class MonitoringExtensions
    {
        /*
         *  Type conversion   
         */
        
        /// <summary>
        /// Converts the target to be of the specified type.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TTo ConvertFast<TFrom, TTo>(this TFrom value)
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
        public static bool TryConvert<TFrom, TTo>(this TFrom value, out TTo result)
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
        
#if UNITY_2020_1_OR_NEWER
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool HasFlagUnsafe<TEnum>(this TEnum lhs, TEnum rhs) where TEnum : unmanaged, Enum
        {
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
        }
#endif
        
        public static bool HasFlag32(this int lhs, int rhs)
        {
            return unchecked ((uint)lhs & (uint)rhs) > 0; 
        }

        /*
         * Color
         */

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string ToRichText(this Color color)
        {
            var sb = ConcurrentStringBuilderPool.Get();
            sb.Append("<color=#");
            sb.Append(ColorUtility.ToHtmlStringRGB(color));
            sb.Append('>');
            return ConcurrentStringBuilderPool.Release(sb);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string Colorize(this string content, Color color)
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
        public static string FontSize(this string content, int size)
        {
            var sb = ConcurrentStringBuilderPool.Get();
            sb.Append("<size=");
            sb.Append(size);
            sb.Append('>');
            sb.Append(content);
            sb.Append("</size>");
            return ConcurrentStringBuilderPool.Release(sb);
        }

        /*
         * RichText   
         */
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string MonoSpace(this string content, int space = 30)
        {
            var sb = ConcurrentStringBuilderPool.Get();
            sb.Append("<mspace=");
            sb.Append(space);
            sb.Append('>');
            sb.Append(content);
            sb.Append("</mspace>");
            return ConcurrentStringBuilderPool.Release(sb);
        }

        /*
         * Text Manipulation   
         */

        public static string NoSpace(this string str)
        {
            return str.Replace(" ", string.Empty);
        }
        
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
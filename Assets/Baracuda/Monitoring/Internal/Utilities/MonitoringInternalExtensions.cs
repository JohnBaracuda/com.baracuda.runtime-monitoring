using System;
using System.Runtime.CompilerServices;
using Baracuda.Monitoring.Internal.Pooling.Concretions;
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
        public static T2 ConvertUnsafe<T1, T2>(this T1 value)
        {
            return UnsafeUtility.As<T1, T2>(ref value);
        }

        /*
         * Enum Flags   
         */
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool HasFlagUnsafe<TEnum>(this TEnum lhs, TEnum rhs) where TEnum : unmanaged, Enum =>
            UnsafeUtility.SizeOf<TEnum>() switch
            {
                1 => (UnsafeUtility.As<TEnum, byte>(ref lhs) & UnsafeUtility.As<TEnum, byte>(ref rhs)) > 0,
                2 => (UnsafeUtility.As<TEnum, ushort>(ref lhs) & UnsafeUtility.As<TEnum, ushort>(ref rhs)) > 0,
                4 => (UnsafeUtility.As<TEnum, uint>(ref lhs) & UnsafeUtility.As<TEnum, uint>(ref rhs)) > 0,
                8 => (UnsafeUtility.As<TEnum, ulong>(ref lhs) & UnsafeUtility.As<TEnum, ulong>(ref rhs)) > 0,
                _ => throw new Exception($"Size of {typeof(TEnum).Name} does not match a known Enum backing type.")
            };

        /*
         * Color   
         */
        
                
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string Colorize(this string content, Color color)
        {
            using var pooled = ConcurrentStringBuilderPool.GetDisposable();
            var str = pooled.Value;
            str.Append("<color=#");
            str.Append(ColorUtility.ToHtmlStringRGBA(color));
            str.Append('>');
            str.Append(content);
            str.Append("</color>");
            return str.ToString();
        }

        /*
         * Text Manipulation   
         */
        
        public static string Humanize(this string target, string[] prefixes = null)
        {
#if UNITY_2021_1_OR_NEWER
            if (IsConst(target))
            {
                return target.Replace('_', ' ').ToLower().ToCamel();
            }
#endif
            
            if (prefixes != null)
            {
                for (var i = 0; i < prefixes.Length; i++)
                {
                    target = target.Replace(prefixes[i], string.Empty);
                }
            }
            
            
            target = target.Replace('_', ' ');
            
            var chars = ListPool<char>.Get();
            
            for (var i = 0; i < target.Length; i++)
            {
                if (i == 0)
                {
                    chars.Add(char.ToUpper(target[i]));
                }
                else
                {
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
            }

            var array = chars.ToArray();
            ListPool<char>.Release(chars);
            return new string(array).ReduceWhitespace();
        }

        public static bool BeginsWith(this string str, char character)
        {
            return !string.IsNullOrWhiteSpace(str) && str[0] == character;
        }
        
#if UNITY_2021_1_OR_NEWER
        private static string ToCamel(this string content)
        {
            Span<char> chars = stackalloc char[content.Length];

            for (var i = 0; i < content.Length; i++)
            {
                var current = content[i];
                var last = i > 0 ? content[i - 1] : ' ';
                chars[i] = last == ' ' ? char.ToUpper(current) : char.ToLower(current);
            }

            return new string(chars);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool IsConst(string input)
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
#endif

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

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Baracuda.Pooling.Concretions;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

namespace Baracuda.Monitoring.Internal.Utils
{
    internal static class Extensions
    {
        #region --- [ENUM] ---

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static bool HasFlagUnsafe<TEnum>(this TEnum lhs, TEnum rhs) where TEnum : unmanaged, Enum =>
            UnsafeUtility.SizeOf<TEnum>() switch
            {
                1 => (UnsafeUtility.As<TEnum, byte>(ref lhs) & UnsafeUtility.As<TEnum, byte>(ref rhs)) > 0,
                2 => (UnsafeUtility.As<TEnum, ushort>(ref lhs) & UnsafeUtility.As<TEnum, ushort>(ref rhs)) > 0,
                4 => (UnsafeUtility.As<TEnum, uint>(ref lhs) & UnsafeUtility.As<TEnum, uint>(ref rhs)) > 0,
                8 => (UnsafeUtility.As<TEnum, ulong>(ref lhs) & UnsafeUtility.As<TEnum, ulong>(ref rhs)) > 0,
                _ => throw new Exception($"Size of {typeof(TEnum).Name} does not match a known Enum backing type.")
            };
        
        public static string AsString(this UnitType target) =>
            target switch
            {
                UnitType.Field => nameof(UnitType.Field),
                UnitType.Property => nameof(UnitType.Property),
                UnitType.Event => nameof(UnitType.Event),
                _ => null
            };
        
        #endregion

        //--------------------------------------------------------------------------------------------------------------

        #region --- [CONVERTION] ---

        /// <summary>
        /// Converts the target to be of the specified <see cref="Type"/>
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static T2 ConvertUnsafe<T1, T2>(this T1 value)
        {
            return UnsafeUtility.As<T1, T2>(ref value);
        }

        #endregion

        //--------------------------------------------------------------------------------------------------------------

        #region --- [STRING OPERATIONS] ---

        internal static string Humanize(this string target, string[] prefixes = null)
        {
            if (prefixes != null)
            {
                for (var i = 0; i < prefixes.Length; i++)
                {
                    target = target.Replace(prefixes[i], string.Empty);
                }
            }

            var chars = new List<char>(target.Length);

            for (var i = 0; i < target.Length; i++)
                if (i == 0)
                {
                    chars.Add(char.ToUpper(target[i]));
                }
                else
                {
                    if (i < target.Length - 1)
                        if (char.IsUpper(target[i]) && !char.IsUpper(target[i + 1])
                            || char.IsUpper(target[i]) && !char.IsUpper(target[i - 1]))
                            if (i > 1)
                                chars.Add(' ');

                    chars.Add(target[i]);
                }

            return new string(chars.ToArray());
        }

        #endregion

        //--------------------------------------------------------------------------------------------------------------

        #region --- [RICHTEXT] ---

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static string Colorize(this string target, Color color)
        {
            var sb = ConcurrentStringBuilderPool.Get();
            sb.Append("<color=#");
            sb.Append(ColorUtility.ToHtmlStringRGBA(color));
            sb.Append('>');
            sb.Append(target);
            sb.Append("</color>");
            return ConcurrentStringBuilderPool.Release(sb);
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static string ToRichTextPrefix(this Color color)
        {
            return $"<color=#{ColorUtility.ToHtmlStringRGBA(color)}>";
        }

        #endregion
    }
}
// Copyright (c) 2022 Jonathan Lang

using System.Runtime.CompilerServices;
using TMPro;
using UnityEngine;

namespace Baracuda.Monitoring.TextMeshPro
{
    internal static class UIExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetParent<T>(this T component, Transform parent) where T : Component
        {
            component.transform.SetParent(parent);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetGameObjectInactive<T>(this T component) where T : Component
        {
            component.gameObject.SetActive(false);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetGameObjectActive<T>(this T component) where T : Component
        {
            component.gameObject.SetActive(true);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetActive<T>(this T component, bool active) where T : Component
        {
            component.gameObject.SetActive(active);
        }

        public static TextAlignmentOptions ToTextAlignmentOptions(this HorizontalTextAlign align)
        {
            switch (align)
            {
                case HorizontalTextAlign.Left:
                    return TextAlignmentOptions.Left;
                case HorizontalTextAlign.Center:
                    return TextAlignmentOptions.Center;
                case HorizontalTextAlign.Right:
                    return TextAlignmentOptions.Right;
            }
            return TextAlignmentOptions.Left;
        }
    }
}
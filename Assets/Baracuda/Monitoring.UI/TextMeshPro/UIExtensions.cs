using System.Runtime.CompilerServices;
using UnityEngine;

namespace Baracuda.Monitoring.UI.TextMeshPro
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
    }
}
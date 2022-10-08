// Copyright (c) 2022 Jonathan Lang


using System;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Scripting;

namespace Baracuda.Monitoring.IL2CPP
{
    /// <summary>
    /// Use this attribute to declare a type definition for an inaccessible value type that you are monitoring in IL2CPP.
    /// </summary>
    [Preserve]
    [AttributeUsage(AttributeTargets.All, AllowMultiple = true)]
    public class TypeDef : Attribute
    {
        /// <summary>
        /// Use this attribute to declare a type definition for an inaccessible value type that you are monitoring in IL2CPP.<b/>
        /// </summary>
        public TypeDef(Type definition)
        {
        }
    }

    /// <summary>
    /// Use this type to declare a type definition for an inaccessible value type that you are monitoring in IL2CPP.
    /// </summary>
    [Preserve]
    public class MonitoredDictionary<TKey, TValue>
    {
#if !DISABLE_MONITORING && (ENABLE_IL2CPP || UNITY_EDITOR)
        [Preserve]
        [MethodImpl(MethodImplOptions.NoOptimization)]
        private static void TypeDefinitions()
        {
            IL2CPPTypeDefinitions.TypeDefDictionary<TKey, TValue>();
            IL2CPPTypeDefinitions.TypeDefField<object, TKey>();
            IL2CPPTypeDefinitions.TypeDefProperty<object, TKey>();
            IL2CPPTypeDefinitions.TypeDefEvent<object, Action<TKey>>();
            IL2CPPTypeDefinitions.TypeDefMethod<object, TKey>();
            IL2CPPTypeDefinitions.TypeDefOutParameter<TKey>();
            IL2CPPTypeDefinitions.TypeDefArray<TKey>();
            IL2CPPTypeDefinitions.TypeDefEnumerable<TKey>();
            IL2CPPTypeDefinitions.TypeDefList<TKey>();
            IL2CPPTypeDefinitions.TypeDefDictionary<TKey, TKey>();
            IL2CPPTypeDefinitions.TypeDefDictionary<object, TKey>();
            IL2CPPTypeDefinitions.TypeDefDictionary<string, TKey>();
            IL2CPPTypeDefinitions.TypeDefDictionary<sbyte, TKey>();
            IL2CPPTypeDefinitions.TypeDefDictionary<byte, TKey>();
            IL2CPPTypeDefinitions.TypeDefDictionary<ushort, TKey>();
            IL2CPPTypeDefinitions.TypeDefDictionary<short, TKey>();
            IL2CPPTypeDefinitions.TypeDefDictionary<uint, TKey>();
            IL2CPPTypeDefinitions.TypeDefDictionary<int, TKey>();
            IL2CPPTypeDefinitions.TypeDefDictionary<ulong, TKey>();
            IL2CPPTypeDefinitions.TypeDefDictionary<long, TKey>();
            IL2CPPTypeDefinitions.TypeDefDictionary<float, TKey>();
            IL2CPPTypeDefinitions.TypeDefDictionary<double, TKey>();
            IL2CPPTypeDefinitions.TypeDefDictionary<decimal, TKey>();
            IL2CPPTypeDefinitions.TypeDefDictionary<char, TKey>();
            IL2CPPTypeDefinitions.TypeDefDictionary<bool, TKey>();
            IL2CPPTypeDefinitions.TypeDefDictionary<DateTime, TKey>();
            IL2CPPTypeDefinitions.TypeDefDictionary<IntPtr, TKey>();
            IL2CPPTypeDefinitions.TypeDefDictionary<UIntPtr, TKey>();
            IL2CPPTypeDefinitions.TypeDefDictionary<Vector2, TKey>();
            IL2CPPTypeDefinitions.TypeDefDictionary<Vector3, TKey>();
            IL2CPPTypeDefinitions.TypeDefDictionary<Vector4, TKey>();
            IL2CPPTypeDefinitions.TypeDefDictionary<Vector2Int, TKey>();
            IL2CPPTypeDefinitions.TypeDefDictionary<Vector3Int, TKey>();
            IL2CPPTypeDefinitions.TypeDefDictionary<Quaternion, TKey>();
            IL2CPPTypeDefinitions.TypeDefDictionary<Color, TKey>();
            IL2CPPTypeDefinitions.TypeDefDictionary<Color32, TKey>();
            IL2CPPTypeDefinitions.TypeDefDictionary<Enum, TKey>();
            IL2CPPTypeDefinitions.TypeDefDictionary<Enum8, TKey>();
            IL2CPPTypeDefinitions.TypeDefDictionary<Enum16, TKey>();
            IL2CPPTypeDefinitions.TypeDefDictionary<Enum32, TKey>();
            IL2CPPTypeDefinitions.TypeDefDictionary<Enum64, TKey>();
            IL2CPPTypeDefinitions.TypeDefDictionary<TKey, object>();
            IL2CPPTypeDefinitions.TypeDefDictionary<TKey, string>();
            IL2CPPTypeDefinitions.TypeDefDictionary<TKey, sbyte>();
            IL2CPPTypeDefinitions.TypeDefDictionary<TKey, byte>();
            IL2CPPTypeDefinitions.TypeDefDictionary<TKey, ushort>();
            IL2CPPTypeDefinitions.TypeDefDictionary<TKey, short>();
            IL2CPPTypeDefinitions.TypeDefDictionary<TKey, uint>();
            IL2CPPTypeDefinitions.TypeDefDictionary<TKey, int>();
            IL2CPPTypeDefinitions.TypeDefDictionary<TKey, ulong>();
            IL2CPPTypeDefinitions.TypeDefDictionary<TKey, long>();
            IL2CPPTypeDefinitions.TypeDefDictionary<TKey, float>();
            IL2CPPTypeDefinitions.TypeDefDictionary<TKey, double>();
            IL2CPPTypeDefinitions.TypeDefDictionary<TKey, decimal>();
            IL2CPPTypeDefinitions.TypeDefDictionary<TKey, char>();
            IL2CPPTypeDefinitions.TypeDefDictionary<TKey, bool>();
            IL2CPPTypeDefinitions.TypeDefDictionary<TKey, DateTime>();
            IL2CPPTypeDefinitions.TypeDefDictionary<TKey, IntPtr>();
            IL2CPPTypeDefinitions.TypeDefDictionary<TKey, UIntPtr>();
            IL2CPPTypeDefinitions.TypeDefDictionary<TKey, Vector2>();
            IL2CPPTypeDefinitions.TypeDefDictionary<TKey, Vector3>();
            IL2CPPTypeDefinitions.TypeDefDictionary<TKey, Vector4>();
            IL2CPPTypeDefinitions.TypeDefDictionary<TKey, Vector2Int>();
            IL2CPPTypeDefinitions.TypeDefDictionary<TKey, Vector3Int>();
            IL2CPPTypeDefinitions.TypeDefDictionary<TKey, Quaternion>();
            IL2CPPTypeDefinitions.TypeDefDictionary<TKey, Color>();
            IL2CPPTypeDefinitions.TypeDefDictionary<TKey, Color32>();
            IL2CPPTypeDefinitions.TypeDefDictionary<TKey, Enum>();
            IL2CPPTypeDefinitions.TypeDefDictionary<TKey, Enum8>();
            IL2CPPTypeDefinitions.TypeDefDictionary<TKey, Enum16>();
            IL2CPPTypeDefinitions.TypeDefDictionary<TKey, Enum32>();
            IL2CPPTypeDefinitions.TypeDefDictionary<TKey, Enum32>();
            IL2CPPTypeDefinitions.TypeDefField<object, TValue>();
            IL2CPPTypeDefinitions.TypeDefProperty<object, TValue>();
            IL2CPPTypeDefinitions.TypeDefEvent<object, Action<TValue>>();
            IL2CPPTypeDefinitions.TypeDefMethod<object, TValue>();
            IL2CPPTypeDefinitions.TypeDefOutParameter<TValue>();
            IL2CPPTypeDefinitions.TypeDefArray<TValue>();
            IL2CPPTypeDefinitions.TypeDefEnumerable<TValue>();
            IL2CPPTypeDefinitions.TypeDefList<TValue>();
            IL2CPPTypeDefinitions.TypeDefDictionary<TValue, TValue>();
            IL2CPPTypeDefinitions.TypeDefDictionary<object, TValue>();
            IL2CPPTypeDefinitions.TypeDefDictionary<string, TValue>();
            IL2CPPTypeDefinitions.TypeDefDictionary<sbyte, TValue>();
            IL2CPPTypeDefinitions.TypeDefDictionary<byte, TValue>();
            IL2CPPTypeDefinitions.TypeDefDictionary<ushort, TValue>();
            IL2CPPTypeDefinitions.TypeDefDictionary<short, TValue>();
            IL2CPPTypeDefinitions.TypeDefDictionary<uint, TValue>();
            IL2CPPTypeDefinitions.TypeDefDictionary<int, TValue>();
            IL2CPPTypeDefinitions.TypeDefDictionary<ulong, TValue>();
            IL2CPPTypeDefinitions.TypeDefDictionary<long, TValue>();
            IL2CPPTypeDefinitions.TypeDefDictionary<float, TValue>();
            IL2CPPTypeDefinitions.TypeDefDictionary<double, TValue>();
            IL2CPPTypeDefinitions.TypeDefDictionary<decimal, TValue>();
            IL2CPPTypeDefinitions.TypeDefDictionary<char, TValue>();
            IL2CPPTypeDefinitions.TypeDefDictionary<bool, TValue>();
            IL2CPPTypeDefinitions.TypeDefDictionary<DateTime, TValue>();
            IL2CPPTypeDefinitions.TypeDefDictionary<IntPtr, TValue>();
            IL2CPPTypeDefinitions.TypeDefDictionary<UIntPtr, TValue>();
            IL2CPPTypeDefinitions.TypeDefDictionary<Vector2, TValue>();
            IL2CPPTypeDefinitions.TypeDefDictionary<Vector3, TValue>();
            IL2CPPTypeDefinitions.TypeDefDictionary<Vector4, TValue>();
            IL2CPPTypeDefinitions.TypeDefDictionary<Vector2Int, TValue>();
            IL2CPPTypeDefinitions.TypeDefDictionary<Vector3Int, TValue>();
            IL2CPPTypeDefinitions.TypeDefDictionary<Quaternion, TValue>();
            IL2CPPTypeDefinitions.TypeDefDictionary<Color, TValue>();
            IL2CPPTypeDefinitions.TypeDefDictionary<Color32, TValue>();
            IL2CPPTypeDefinitions.TypeDefDictionary<Enum, TValue>();
            IL2CPPTypeDefinitions.TypeDefDictionary<Enum8, TValue>();
            IL2CPPTypeDefinitions.TypeDefDictionary<Enum16, TValue>();
            IL2CPPTypeDefinitions.TypeDefDictionary<Enum32, TValue>();
            IL2CPPTypeDefinitions.TypeDefDictionary<Enum64, TValue>();
            IL2CPPTypeDefinitions.TypeDefDictionary<TValue, object>();
            IL2CPPTypeDefinitions.TypeDefDictionary<TValue, string>();
            IL2CPPTypeDefinitions.TypeDefDictionary<TValue, sbyte>();
            IL2CPPTypeDefinitions.TypeDefDictionary<TValue, byte>();
            IL2CPPTypeDefinitions.TypeDefDictionary<TValue, ushort>();
            IL2CPPTypeDefinitions.TypeDefDictionary<TValue, short>();
            IL2CPPTypeDefinitions.TypeDefDictionary<TValue, uint>();
            IL2CPPTypeDefinitions.TypeDefDictionary<TValue, int>();
            IL2CPPTypeDefinitions.TypeDefDictionary<TValue, ulong>();
            IL2CPPTypeDefinitions.TypeDefDictionary<TValue, long>();
            IL2CPPTypeDefinitions.TypeDefDictionary<TValue, float>();
            IL2CPPTypeDefinitions.TypeDefDictionary<TValue, double>();
            IL2CPPTypeDefinitions.TypeDefDictionary<TValue, decimal>();
            IL2CPPTypeDefinitions.TypeDefDictionary<TValue, char>();
            IL2CPPTypeDefinitions.TypeDefDictionary<TValue, bool>();
            IL2CPPTypeDefinitions.TypeDefDictionary<TValue, DateTime>();
            IL2CPPTypeDefinitions.TypeDefDictionary<TValue, IntPtr>();
            IL2CPPTypeDefinitions.TypeDefDictionary<TValue, UIntPtr>();
            IL2CPPTypeDefinitions.TypeDefDictionary<TValue, Vector2>();
            IL2CPPTypeDefinitions.TypeDefDictionary<TValue, Vector3>();
            IL2CPPTypeDefinitions.TypeDefDictionary<TValue, Vector4>();
            IL2CPPTypeDefinitions.TypeDefDictionary<TValue, Vector2Int>();
            IL2CPPTypeDefinitions.TypeDefDictionary<TValue, Vector3Int>();
            IL2CPPTypeDefinitions.TypeDefDictionary<TValue, Quaternion>();
            IL2CPPTypeDefinitions.TypeDefDictionary<TValue, Color>();
            IL2CPPTypeDefinitions.TypeDefDictionary<TValue, Color32>();
            IL2CPPTypeDefinitions.TypeDefDictionary<TValue, Enum>();
            IL2CPPTypeDefinitions.TypeDefDictionary<TValue, Enum8>();
            IL2CPPTypeDefinitions.TypeDefDictionary<TValue, Enum16>();
            IL2CPPTypeDefinitions.TypeDefDictionary<TValue, Enum32>();
            IL2CPPTypeDefinitions.TypeDefDictionary<TValue, Enum32>();
        }
#endif
    }

    /// <summary>
    /// Use this type to declare a type definition for an inaccessible value type that you are monitoring in IL2CPP.
    /// </summary>
    [Preserve]
    public class MonitoredStruct<T> where T : struct
    {
#if !DISABLE_MONITORING && (ENABLE_IL2CPP || UNITY_EDITOR)
        [Preserve]
        [MethodImpl(MethodImplOptions.NoOptimization)]
        private static void TypeDefinitions()
        {
            IL2CPPTypeDefinitions.TypeDefField<object, T>();
            IL2CPPTypeDefinitions.TypeDefProperty<object, T>();
            IL2CPPTypeDefinitions.TypeDefEvent<object, Action<T>>();
            IL2CPPTypeDefinitions.TypeDefMethod<object, T>();
            IL2CPPTypeDefinitions.TypeDefOutParameter<T>();
            IL2CPPTypeDefinitions.TypeDefStructTypeArray<T>();
            IL2CPPTypeDefinitions.TypeDefArray<T>();
            IL2CPPTypeDefinitions.TypeDefEnumerable<T>();
            IL2CPPTypeDefinitions.TypeDefList<T>();
            IL2CPPTypeDefinitions.TypeDefDictionary<T, T>();
            IL2CPPTypeDefinitions.TypeDefDictionary<object, T>();
            IL2CPPTypeDefinitions.TypeDefDictionary<string, T>();
            IL2CPPTypeDefinitions.TypeDefDictionary<sbyte, T>();
            IL2CPPTypeDefinitions.TypeDefDictionary<byte, T>();
            IL2CPPTypeDefinitions.TypeDefDictionary<ushort, T>();
            IL2CPPTypeDefinitions.TypeDefDictionary<short, T>();
            IL2CPPTypeDefinitions.TypeDefDictionary<uint, T>();
            IL2CPPTypeDefinitions.TypeDefDictionary<int, T>();
            IL2CPPTypeDefinitions.TypeDefDictionary<ulong, T>();
            IL2CPPTypeDefinitions.TypeDefDictionary<long, T>();
            IL2CPPTypeDefinitions.TypeDefDictionary<float, T>();
            IL2CPPTypeDefinitions.TypeDefDictionary<double, T>();
            IL2CPPTypeDefinitions.TypeDefDictionary<decimal, T>();
            IL2CPPTypeDefinitions.TypeDefDictionary<char, T>();
            IL2CPPTypeDefinitions.TypeDefDictionary<bool, T>();
            IL2CPPTypeDefinitions.TypeDefDictionary<DateTime, T>();
            IL2CPPTypeDefinitions.TypeDefDictionary<IntPtr, T>();
            IL2CPPTypeDefinitions.TypeDefDictionary<UIntPtr, T>();
            IL2CPPTypeDefinitions.TypeDefDictionary<Vector2, T>();
            IL2CPPTypeDefinitions.TypeDefDictionary<Vector3, T>();
            IL2CPPTypeDefinitions.TypeDefDictionary<Vector4, T>();
            IL2CPPTypeDefinitions.TypeDefDictionary<Vector2Int, T>();
            IL2CPPTypeDefinitions.TypeDefDictionary<Vector3Int, T>();
            IL2CPPTypeDefinitions.TypeDefDictionary<Quaternion, T>();
            IL2CPPTypeDefinitions.TypeDefDictionary<Color, T>();
            IL2CPPTypeDefinitions.TypeDefDictionary<Color32, T>();
            IL2CPPTypeDefinitions.TypeDefDictionary<Enum, T>();
            IL2CPPTypeDefinitions.TypeDefDictionary<Enum8, T>();
            IL2CPPTypeDefinitions.TypeDefDictionary<Enum16, T>();
            IL2CPPTypeDefinitions.TypeDefDictionary<Enum32, T>();
            IL2CPPTypeDefinitions.TypeDefDictionary<Enum64, T>();
            IL2CPPTypeDefinitions.TypeDefDictionary<T, object>();
            IL2CPPTypeDefinitions.TypeDefDictionary<T, string>();
            IL2CPPTypeDefinitions.TypeDefDictionary<T, sbyte>();
            IL2CPPTypeDefinitions.TypeDefDictionary<T, byte>();
            IL2CPPTypeDefinitions.TypeDefDictionary<T, ushort>();
            IL2CPPTypeDefinitions.TypeDefDictionary<T, short>();
            IL2CPPTypeDefinitions.TypeDefDictionary<T, uint>();
            IL2CPPTypeDefinitions.TypeDefDictionary<T, int>();
            IL2CPPTypeDefinitions.TypeDefDictionary<T, ulong>();
            IL2CPPTypeDefinitions.TypeDefDictionary<T, long>();
            IL2CPPTypeDefinitions.TypeDefDictionary<T, float>();
            IL2CPPTypeDefinitions.TypeDefDictionary<T, double>();
            IL2CPPTypeDefinitions.TypeDefDictionary<T, decimal>();
            IL2CPPTypeDefinitions.TypeDefDictionary<T, char>();
            IL2CPPTypeDefinitions.TypeDefDictionary<T, bool>();
            IL2CPPTypeDefinitions.TypeDefDictionary<T, DateTime>();
            IL2CPPTypeDefinitions.TypeDefDictionary<T, IntPtr>();
            IL2CPPTypeDefinitions.TypeDefDictionary<T, UIntPtr>();
            IL2CPPTypeDefinitions.TypeDefDictionary<T, Vector2>();
            IL2CPPTypeDefinitions.TypeDefDictionary<T, Vector3>();
            IL2CPPTypeDefinitions.TypeDefDictionary<T, Vector4>();
            IL2CPPTypeDefinitions.TypeDefDictionary<T, Vector2Int>();
            IL2CPPTypeDefinitions.TypeDefDictionary<T, Vector3Int>();
            IL2CPPTypeDefinitions.TypeDefDictionary<T, Quaternion>();
            IL2CPPTypeDefinitions.TypeDefDictionary<T, Color>();
            IL2CPPTypeDefinitions.TypeDefDictionary<T, Color32>();
            IL2CPPTypeDefinitions.TypeDefDictionary<T, Enum>();
            IL2CPPTypeDefinitions.TypeDefDictionary<T, Enum8>();
            IL2CPPTypeDefinitions.TypeDefDictionary<T, Enum16>();
            IL2CPPTypeDefinitions.TypeDefDictionary<T, Enum32>();
            IL2CPPTypeDefinitions.TypeDefDictionary<T, Enum32>();
        }
#endif
    }
}
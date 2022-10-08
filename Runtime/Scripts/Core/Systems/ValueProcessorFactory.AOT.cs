// Copyright (c) 2022 Jonathan Lang

#if ENABLE_IL2CPP || UNITY_EDITOR

using Baracuda.Monitoring.IL2CPP;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Scripting;

namespace Baracuda.Monitoring.Systems
{
    internal partial class ValueProcessorFactory
    {
        #region AOT ---

        [Preserve]
        [MethodImpl(MethodImplOptions.NoOptimization)]
        internal static void AOTList<TList, TElement>() where TList : IList<TElement>
        {
            GuardAOTRuntimeMethodCall();
            CreateIListFuncWithoutIndexArgument<TList, TElement>(null, null);
            CreateIListFuncWithIndexArgument<TList, TElement>(null, null);
        }

        [Preserve]
        [MethodImpl(MethodImplOptions.NoOptimization)]
        internal static void AOTValueTypeArray<T>() where T : struct
        {
            GuardAOTRuntimeMethodCall();
            ValueTypeArrayProcessor<T>(null);
        }

        [Preserve]
        [MethodImpl(MethodImplOptions.NoOptimization)]
        internal static void AOTReferenceTypeArray<T>()
        {
            GuardAOTRuntimeMethodCall();
            ReferenceTypeArrayProcessor<T>(null);
        }

        [Preserve]
        [MethodImpl(MethodImplOptions.NoOptimization)]
        internal static void AOTDictionary<TKey, TValue>()
        {
            GuardAOTRuntimeMethodCall();
            DictionaryProcessor<TKey, TValue>(null);
            CreateIDictionaryFunc<Dictionary<TKey, TValue>, TKey, TValue>(null, null);
            CreateIDictionaryFunc<IDictionary<TKey, TValue>, TKey, TValue>(null, null);
        }

        [Preserve]
        [MethodImpl(MethodImplOptions.NoOptimization)]
        internal static void AOTEnumerable<T>()
        {
            GuardAOTRuntimeMethodCall();
            GenericIEnumerableProcessor<T>(null);
        }

        [Preserve]
        private static void GuardAOTRuntimeMethodCall()
        {
            throw new InvalidOperationException("Illegal AOT Method call!");
        }

        // Note: this method just handles common types. Custom type implementation will be added soon.
        [Preserve]
        [MethodImpl(MethodImplOptions.NoOptimization)]
        private static void __AOT()
        {
            GuardAOTRuntimeMethodCall();

            GenericIEnumerableProcessor<object>(null);
            GenericIEnumerableProcessor<string>(null);
            GenericIEnumerableProcessor<dynamic>(null);
            GenericIEnumerableProcessor<bool>(null);
            GenericIEnumerableProcessor<byte>(null);
            GenericIEnumerableProcessor<sbyte>(null);
            GenericIEnumerableProcessor<char>(null);
            GenericIEnumerableProcessor<decimal>(null);
            GenericIEnumerableProcessor<double>(null);
            GenericIEnumerableProcessor<float>(null);
            GenericIEnumerableProcessor<int>(null);
            GenericIEnumerableProcessor<uint>(null);
            GenericIEnumerableProcessor<long>(null);
            GenericIEnumerableProcessor<ulong>(null);
            GenericIEnumerableProcessor<IntPtr>(null);
            GenericIEnumerableProcessor<UIntPtr>(null);
            GenericIEnumerableProcessor<short>(null);
            GenericIEnumerableProcessor<ushort>(null);
            GenericIEnumerableProcessor<DateTime>(null);
            GenericIEnumerableProcessor<Enum8>(null);
            GenericIEnumerableProcessor<Enum16>(null);
            GenericIEnumerableProcessor<Enum32>(null);
            GenericIEnumerableProcessor<Enum64>(null);
            GenericIEnumerableProcessor<Vector2>(null);
            GenericIEnumerableProcessor<Vector3>(null);
            GenericIEnumerableProcessor<Vector4>(null);
            GenericIEnumerableProcessor<Vector2Int>(null);
            GenericIEnumerableProcessor<Vector3Int>(null);
            GenericIEnumerableProcessor<Quaternion>(null);
            GenericIEnumerableProcessor<Color>(null);
            GenericIEnumerableProcessor<Color32>(null);

            ReferenceTypeArrayProcessor<object>(null);
            ReferenceTypeArrayProcessor<string>(null);
            ReferenceTypeArrayProcessor<dynamic>(null);
            ValueTypeArrayProcessor<bool>(null);
            ValueTypeArrayProcessor<byte>(null);
            ValueTypeArrayProcessor<sbyte>(null);
            ValueTypeArrayProcessor<char>(null);
            ValueTypeArrayProcessor<decimal>(null);
            ValueTypeArrayProcessor<double>(null);
            ValueTypeArrayProcessor<float>(null);
            ValueTypeArrayProcessor<int>(null);
            ValueTypeArrayProcessor<uint>(null);
            ValueTypeArrayProcessor<long>(null);
            ValueTypeArrayProcessor<ulong>(null);
            ValueTypeArrayProcessor<IntPtr>(null);
            ValueTypeArrayProcessor<UIntPtr>(null);
            ValueTypeArrayProcessor<short>(null);
            ValueTypeArrayProcessor<ushort>(null);
            ValueTypeArrayProcessor<DateTime>(null);
            ValueTypeArrayProcessor<Enum8>(null);
            ValueTypeArrayProcessor<Enum16>(null);
            ValueTypeArrayProcessor<Enum32>(null);
            ValueTypeArrayProcessor<Enum64>(null);
            ValueTypeArrayProcessor<Vector2>(null);
            ValueTypeArrayProcessor<Vector3>(null);
            ValueTypeArrayProcessor<Vector4>(null);
            ValueTypeArrayProcessor<Vector2Int>(null);
            ValueTypeArrayProcessor<Vector3Int>(null);
            ValueTypeArrayProcessor<Quaternion>(null);
            ValueTypeArrayProcessor<Color>(null);
            ValueTypeArrayProcessor<Color32>(null);

            DictionaryProcessor<object, object>(null);
            DictionaryProcessor<object, bool>(null);
            DictionaryProcessor<object, byte>(null);
            DictionaryProcessor<object, sbyte>(null);
            DictionaryProcessor<object, char>(null);
            DictionaryProcessor<object, decimal>(null);
            DictionaryProcessor<object, double>(null);
            DictionaryProcessor<object, float>(null);
            DictionaryProcessor<object, int>(null);
            DictionaryProcessor<object, uint>(null);
            DictionaryProcessor<object, long>(null);
            DictionaryProcessor<object, ulong>(null);
            DictionaryProcessor<object, IntPtr>(null);
            DictionaryProcessor<object, UIntPtr>(null);
            DictionaryProcessor<object, short>(null);
            DictionaryProcessor<object, ushort>(null);
            DictionaryProcessor<object, DateTime>(null);
            DictionaryProcessor<object, Enum8>(null);
            DictionaryProcessor<object, Enum16>(null);
            DictionaryProcessor<object, Enum32>(null);
            DictionaryProcessor<object, Enum64>(null);
            DictionaryProcessor<object, Vector2>(null);
            DictionaryProcessor<object, Vector3>(null);
            DictionaryProcessor<object, Vector4>(null);
            DictionaryProcessor<object, Vector2Int>(null);
            DictionaryProcessor<object, Vector3Int>(null);
            DictionaryProcessor<object, Quaternion>(null);
            DictionaryProcessor<object, Color>(null);
            DictionaryProcessor<object, Color32>(null);
            DictionaryProcessor<dynamic, dynamic>(null);
        }

        #endregion
    }
}
#endif

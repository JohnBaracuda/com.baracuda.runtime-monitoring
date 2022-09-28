// Copyright (c) 2022 Jonathan Lang

#if ENABLE_IL2CPP || UNITY_EDITOR
using Baracuda.Monitoring.Profiles;
using Baracuda.Monitoring.Systems;
using Baracuda.Monitoring.Types;
using Baracuda.Monitoring.Units;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine.Scripting;

namespace Baracuda.Monitoring.IL2CPP
{
    public class TypeDefDictionary<TKey, TValue>
    {
        private static void __Def()
        {
        }
    }

    public class TypeDef<T>
    {
        private static void __Def()
        {
            IL2CPPTypeDefinitions.TypeDefOutParameter<T>();
        }
    }

    public class IL2CPPTypeDef : Attribute
    {
        public IL2CPPTypeDef(Type definition)
        {
        }
    }

    public static class IL2CPPTypeDefinitions
    {
        [IL2CPPTypeDef(typeof(TypeDefDictionary<int, int>))]
        [Preserve]
        [MethodImpl(MethodImplOptions.NoOptimization)]
        public static void TypeDefStructTypeArray<T>() where T : struct
        {
            ValueProcessorFactory.AOTValueTypeArray<T>();
        }

        [Preserve]
        [MethodImpl(MethodImplOptions.NoOptimization)]
        public static void TypeDefArray<T>()
        {
            ValueProcessorFactory.AOTReferenceTypeArray<T>();
        }

        [Preserve]
        [MethodImpl(MethodImplOptions.NoOptimization)]
        public static void TypeDefDictionary<TKey, TValue>()
        {
            ValueProcessorFactory.AOTDictionary<TKey, TValue>();
        }

        [Preserve]
        [MethodImpl(MethodImplOptions.NoOptimization)]
        public static void TypeDefEnumerable<T>()
        {
            ValueProcessorFactory.AOTEnumerable<T>();
        }

        [Preserve]
        [MethodImpl(MethodImplOptions.NoOptimization)]
        public static void TypeDefList<TList, TElement>() where TList : IList<TElement>
        {
            ValueProcessorFactory.AOTList<TList, TElement>();
        }

        [Preserve]
        [MethodImpl(MethodImplOptions.NoOptimization)]
        public static void TypeDefField<TDeclaring, TMonitored>() where TDeclaring : class
        {
            DeclareThrow((FieldUnit<TDeclaring, TMonitored>) null);
            DeclareThrow((FieldProfile<TDeclaring, TMonitored>) null);
        }

        [Preserve]
        [MethodImpl(MethodImplOptions.NoOptimization)]
        public static void TypeDefProperty<TDeclaring, TMonitored>() where TDeclaring : class
        {
            DeclareThrow((PropertyUnit<TDeclaring, TMonitored>) null);
            DeclareThrow((PropertyProfile<TDeclaring, TMonitored>) null);
        }

        [Preserve]
        [MethodImpl(MethodImplOptions.NoOptimization)]
        public static void TypeDefEvent<TDeclaring, TMonitored>() where TDeclaring : class where TMonitored : Delegate
        {
            DeclareThrow((EventUnit<TDeclaring, TMonitored>) null);
            DeclareThrow((EventProfile<TDeclaring, TMonitored>) null);
        }

        [Preserve]
        [MethodImpl(MethodImplOptions.NoOptimization)]
        public static void TypeDefMethod<TDeclaring, TMonitored>() where TDeclaring : class where TMonitored : Delegate
        {
            DeclareThrow((MethodUnit<TDeclaring, TMonitored>) null);
            DeclareThrow((MethodProfile<TDeclaring, TMonitored>) null);
        }

        [Preserve]
        [MethodImpl(MethodImplOptions.NoOptimization)]
        public static void TypeDefOutParameter<T>()
        {
            DeclareThrow((OutParameterHandleT<T>) null);
        }

        [Preserve]
        [MethodImpl(MethodImplOptions.NoOptimization)]
        private static void DeclareThrow<T>(T obj)
        {
            throw new InvalidOperationException("Illegal AOT Method call!");
        }
    }
}
#endif

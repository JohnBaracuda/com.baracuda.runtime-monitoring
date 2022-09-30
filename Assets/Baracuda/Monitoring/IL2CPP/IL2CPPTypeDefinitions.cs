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
    public static class IL2CPPTypeDefinitions
    {
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
        public static void TypeDefList<TElement>()
        {
            ValueProcessorFactory.AOTList<List<TElement>, TElement>();
            ValueProcessorFactory.AOTList<IList<TElement>, TElement>();
        }

        [Preserve]
        [MethodImpl(MethodImplOptions.NoOptimization)]
        public static void TypeDefField<TDeclaring, TMonitored>() where TDeclaring : class
        {
            var unit = Activator.CreateInstance<FieldUnit<TDeclaring, TMonitored>>();
            var profile = Activator.CreateInstance<FieldProfile<TDeclaring, TMonitored>>();
            DeclareThrow(unit);
            DeclareThrow(profile);
        }

        [Preserve]
        [MethodImpl(MethodImplOptions.NoOptimization)]
        public static void TypeDefProperty<TDeclaring, TMonitored>() where TDeclaring : class
        {
            var unit = Activator.CreateInstance<PropertyUnit<TDeclaring, TMonitored>>();
            var profile = Activator.CreateInstance<PropertyProfile<TDeclaring, TMonitored>>();
            DeclareThrow(unit);
            DeclareThrow(profile);
        }

        [Preserve]
        [MethodImpl(MethodImplOptions.NoOptimization)]
        public static void TypeDefEvent<TDeclaring, TMonitored>() where TDeclaring : class where TMonitored : Delegate
        {
            var unit = Activator.CreateInstance<EventUnit<TDeclaring, TMonitored>>();
            var profile = Activator.CreateInstance<EventProfile<TDeclaring, TMonitored>>();
            DeclareThrow(unit);
            DeclareThrow(profile);
        }

        // Method

        [Preserve]
        [MethodImpl(MethodImplOptions.NoOptimization)]
        public static void TypeDefMethod<TDeclaring>() where TDeclaring : class
        {
            var unit = Activator.CreateInstance<MethodUnit<TDeclaring, __Void>>();
            var profile = Activator.CreateInstance<MethodProfile<TDeclaring, __Void>>();
            DeclareThrow(unit);
            DeclareThrow(profile);
        }

        [Preserve]
        [MethodImpl(MethodImplOptions.NoOptimization)]
        public static void TypeDefMethod<TDeclaring, TMonitored>() where TDeclaring : class
        {
            var unit = Activator.CreateInstance<MethodUnit<TDeclaring, TMonitored>>();
            var profile = Activator.CreateInstance<MethodProfile<TDeclaring, TMonitored>>();
            DeclareThrow(unit);
            DeclareThrow(profile);
        }

        [Preserve]
        [MethodImpl(MethodImplOptions.NoOptimization)]
        public static void TypeDefOutParameter<T>()
        {
            var handle = Activator.CreateInstance<OutParameterHandleT<T>>();
            DeclareThrow(handle);
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

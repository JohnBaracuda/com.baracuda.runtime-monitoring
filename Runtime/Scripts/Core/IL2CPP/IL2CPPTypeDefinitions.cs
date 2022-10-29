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
using Void = Baracuda.Monitoring.Types.Void;

namespace Baracuda.Monitoring.IL2CPP
{
    /// <summary>
    /// Class is used to create type definitions for IL2CPP.<br/>
    /// Do not use call API of this class manually!
    /// </summary>
    public static class IL2CPPTypeDefinitions
    {
        /// <summary>
        /// Do not call this method manually!
        /// </summary>
        [Preserve]
        [MethodImpl(MethodImplOptions.NoOptimization)]
        public static void TypeDefStructTypeArray<T>() where T : struct
        {
            ValueProcessorFactory.AOTValueTypeArray<T>();
        }

        /// <summary>
        /// Do not call this method manually!
        /// </summary>
        [Preserve]
        [MethodImpl(MethodImplOptions.NoOptimization)]
        public static void TypeDefArray<T>()
        {
            ValueProcessorFactory.AOTReferenceTypeArray<T>();
        }

        /// <summary>
        /// Do not call this method manually!
        /// </summary>
        [Preserve]
        [MethodImpl(MethodImplOptions.NoOptimization)]
        public static void TypeDefDictionary<TKey, TValue>()
        {
            ValueProcessorFactory.AOTDictionary<TKey, TValue>();
        }

        /// <summary>
        /// Do not call this method manually!
        /// </summary>
        [Preserve]
        [MethodImpl(MethodImplOptions.NoOptimization)]
        public static void TypeDefEnumerable<T>()
        {
            ValueProcessorFactory.AOTEnumerable<T>();
        }

        /// <summary>
        /// Do not call this method manually!
        /// </summary>
        [Preserve]
        [MethodImpl(MethodImplOptions.NoOptimization)]
        public static void TypeDefList<TList, TElement>() where TList : IList<TElement>
        {
            ValueProcessorFactory.AOTList<TList, TElement>();
        }

        /// <summary>
        /// Do not call this method manually!
        /// </summary>
        [Preserve]
        [MethodImpl(MethodImplOptions.NoOptimization)]
        public static void TypeDefList<TElement>()
        {
            ValueProcessorFactory.AOTList<List<TElement>, TElement>();
            ValueProcessorFactory.AOTList<IList<TElement>, TElement>();
        }

        /// <summary>
        /// Do not call this method manually!
        /// </summary>
        [Preserve]
        [MethodImpl(MethodImplOptions.NoOptimization)]
        public static void TypeDefField<TDeclaring, TMonitored>() where TDeclaring : class
        {
            var unit = Activator.CreateInstance<FieldHandle<TDeclaring, TMonitored>>();
            var profile = Activator.CreateInstance<FieldProfile<TDeclaring, TMonitored>>();
            DeclareThrow(unit);
            DeclareThrow(profile);
        }

        /// <summary>
        /// Do not call this method manually!
        /// </summary>
        [Preserve]
        [MethodImpl(MethodImplOptions.NoOptimization)]
        public static void TypeDefProperty<TDeclaring, TMonitored>() where TDeclaring : class
        {
            var unit = Activator.CreateInstance<PropertyHandle<TDeclaring, TMonitored>>();
            var profile = Activator.CreateInstance<PropertyProfile<TDeclaring, TMonitored>>();
            DeclareThrow(unit);
            DeclareThrow(profile);
        }

        /// <summary>
        /// Do not call this method manually!
        /// </summary>
        [Preserve]
        [MethodImpl(MethodImplOptions.NoOptimization)]
        public static void TypeDefEvent<TDeclaring, TMonitored>() where TDeclaring : class where TMonitored : Delegate
        {
            var unit = Activator.CreateInstance<EventHandle<TDeclaring, TMonitored>>();
            var profile = Activator.CreateInstance<EventProfile<TDeclaring, TMonitored>>();
            DeclareThrow(unit);
            DeclareThrow(profile);
        }

        // Method

        /// <summary>
        /// Do not call this method manually!
        /// </summary>
        [Preserve]
        [MethodImpl(MethodImplOptions.NoOptimization)]
        public static void TypeDefMethod<TDeclaring>() where TDeclaring : class
        {
            var unit = Activator.CreateInstance<MethodHandle<TDeclaring, Void>>();
            var profile = Activator.CreateInstance<MethodProfile<TDeclaring, Void>>();
            DeclareThrow(unit);
            DeclareThrow(profile);
        }

        /// <summary>
        /// Do not call this method manually!
        /// </summary>
        [Preserve]
        [MethodImpl(MethodImplOptions.NoOptimization)]
        public static void TypeDefMethod<TDeclaring, TMonitored>() where TDeclaring : class
        {
            var unit = Activator.CreateInstance<MethodHandle<TDeclaring, TMonitored>>();
            var profile = Activator.CreateInstance<MethodProfile<TDeclaring, TMonitored>>();
            DeclareThrow(unit);
            DeclareThrow(profile);
        }

        /// <summary>
        /// Do not call this method manually!
        /// </summary>
        [Preserve]
        [MethodImpl(MethodImplOptions.NoOptimization)]
        public static void TypeDefOutParameter<T>()
        {
            var handle = Activator.CreateInstance<OutParameterHandleT<T>>();
            DeclareThrow(handle);
        }

        /// <summary>
        /// Do not call this method manually!
        /// </summary>
        [Preserve]
        [MethodImpl(MethodImplOptions.NoOptimization)]
        private static void DeclareThrow<T>(T obj)
        {
            throw new InvalidOperationException("Illegal AOT Method call!");
        }
    }
}
#endif

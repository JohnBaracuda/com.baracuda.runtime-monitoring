// Copyright (c) 2022 Jonathan Lang
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using Baracuda.Monitoring.API;
using Baracuda.Monitoring.Internal.Exceptions;
using Baracuda.Monitoring.Internal.Units;
using Baracuda.Monitoring.Internal.Utilities;
using Baracuda.Reflection;
using UnityEngine;

namespace Baracuda.Monitoring.Internal.Profiling
{
    /// <summary>
    /// Class creates custom ValueProcessor delegates for Monitoring units based on their values type.
    /// </summary>
    internal static partial class ValueProcessorFactory
    {
        #region --- API ---

        /// <summary>
        /// Creates a default type specific processor to format the <see cref="TValue"/> depending on its exact type.
        /// </summary>
        /// <param name="profile">The target <see cref="MonitorProfile"/></param>
        /// <typeparam name="TValue">The type of the value that should be parsed/formatted</typeparam>
        /// <returns></returns>
        internal static Func<TValue, string> CreateTypeSpecificProcessor<TValue>(MonitorProfile profile)
        {
            return CreateTypeSpecificProcessorInternal<TValue>(profile);
        }
        
        #endregion
        
        //--------------------------------------------------------------------------------------------------------------

        #region --- Internal ---

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Func<TValue, string> CreateTypeSpecificProcessorInternal<TValue>(MonitorProfile profile)
        {
            // Transform
            if (profile.UnitValueType == typeof(Transform))
            {
                return (Func<TValue, string>)(Delegate) TransformProcessor(profile);
            }
            
            // Boolean
            if (profile.UnitValueType == typeof(bool))
            {
                return (Func<TValue, string>)(Delegate) CreateBooleanProcessor(profile);
            }
            
#if !ENABLE_IL2CPP
            // Dictionary<TKey, TValue>
            if (profile.UnitValueType.IsGenericIDictionary())
            {
                var keyType   = profile.UnitValueType.GetGenericArguments()[0];
                var valueType = profile.UnitValueType.GetGenericArguments()[1];
                var genericMethod = createDictionaryProcessorMethod.MakeGenericMethod(keyType, valueType);
                return (Func<TValue, string>) genericMethod.Invoke(null, new object[]{profile});
            }
#endif

            // IEnumerable<bool>
            if (profile.UnitValueType.HasInterface<IEnumerable<bool>>())
            {
                return (Func<TValue, string>) (Delegate) IEnumerableBooleanProcessor(profile);
            }
            
#if !ENABLE_IL2CPP
            // IEnumerable<T>
            if (profile.UnitValueType.IsGenericIEnumerable(true))
            {
                var type = profile.UnitValueType.GetElementType() ?? profile.UnitValueType.GetGenericArguments()[0];
                var genericMethod = createGenericIEnumerableMethod.MakeGenericMethod(type);
                return (Func<TValue, string>) genericMethod.Invoke(null, new object[]{profile});
            }

            // Array<T>
            if (profile.UnitValueType.IsArray)
            {
                var type = profile.UnitValueType.GetElementType();

                Debug.Assert(type != null, nameof(type) + " != null");
                
                var genericMethod = type.IsValueType ? createValueTypeArrayMethod.MakeGenericMethod(type) : createReferenceTypeArrayMethod.MakeGenericMethod(type);
                
                return (Func<TValue, string>) genericMethod.Invoke(null, new object[]{profile});
            }
#endif
            
            // IEnumerable
            if (profile.UnitValueType.IsIEnumerable(true))
            {
                return (Func<TValue, string>) (Delegate) IEnumerableProcessor(profile);
            }

            // Quaternion
            if (profile.UnitValueType == typeof(Quaternion))
            {
                return (Func<TValue, string>) (Delegate) QuaternionProcessor(profile);
            }

            // Vector3
            if (profile.UnitValueType == typeof(Vector3))
            {
                return (Func<TValue, string>) (Delegate) Vector3Processor(profile);
            }
            
            // Vector2
            if (profile.UnitValueType == typeof(Vector2))
            {
                return (Func<TValue, string>) (Delegate) Vector2Processor(profile);
            }

            // Color
            if (profile.UnitValueType == typeof(Color))
            {
                return (Func<TValue, string>) (Delegate) ColorProcessor(profile);
            }
            
            // Color32
            if (profile.UnitValueType == typeof(Color32))
            {
                return (Func<TValue, string>) (Delegate) Color32Processor(profile);
            }

            // Format
            if (profile.UnitValueType.HasInterface<IFormattable>() && profile.FormatData.Format != null)
            {
                return FormattedProcessor<TValue>(profile);
            }
            
            // UnityEngine.Object
            if (profile.UnitValueType.IsSubclassOf(typeof(UnityEngine.Object)))
            {
                return (Func<TValue, string>) (Delegate) UnityEngineObjectProcessor(profile);
            }
            
            // Int32
            if (profile.UnitValueType.IsInt32())
            {
                return (Func<TValue, string>) (Delegate) Int32Processor(profile);
            }
            
            // Int64
            if (profile.UnitValueType.IsInt64())
            {
                return (Func<TValue, string>) (Delegate) Int64Processor(profile);
            }
            
            // Float
            if (profile.UnitValueType.IsSingle())
            {
                return (Func<TValue, string>) (Delegate) SingleProcessor(profile);
            }
            
            // Double
            if (profile.UnitValueType.IsDouble())
            {
                return (Func<TValue, string>) (Delegate) DoubleProcessor(profile);
            }

            // Value Type
            if (profile.UnitValueType.IsValueType)
            {
                return ValueTypeProcessor<TValue>(profile);
            }
            
            // Reference Type
            else
            {
                return ReferenceTypeProcessor<TValue>(profile);
            }
        }

        #endregion
        
        //--------------------------------------------------------------------------------------------------------------

        #region --- Value Processor: Ilist With Index Argument ---

        private static readonly MethodInfo genericIListWithIndexCreateMethod =
            typeof(ValueProcessorFactory).GetMethod(nameof(CreateIListFuncWithIndexArgument), STATIC_FLAGS);
        
        /// <summary>
        /// Creates a delegate that accepts an input of type <see cref="IList{TElement}"/> and returns a string, using
        /// a custom value processor with a signature <see cref="string"/>(<see cref="TElement"/> element <see cref="Int32"/> index)<br/>
        /// </summary>
        /// <param name="processor">the <see cref="MethodInfo"/> of the previously validated processor</param>
        /// <param name="name">the name of the <see cref="ValueUnit{TTarget,TValue}"/></param>
        /// <typeparam name="TInput">the exact argument/input type of the processors method. This type must be assignable from <see cref="IList{TElement}"/></typeparam>
        /// <typeparam name="TElement">the element type of the IList</typeparam>
        /// <returns></returns>
        private static Func<TInput, string> CreateIListFuncWithIndexArgument<TInput, TElement>(MethodInfo processor, string name) where TInput : IList<TElement>
        {
            try
            {
                // create a matching delegate with the processors method info.
                var listDelegate = (Func<TElement, int, string>)Delegate.CreateDelegate(typeof(Func<TElement, int, string>), processor);
                
                // create a matching null string.
                var nullString = $"{name}: {NULL}";
                
                // create a stringBuilder object to be used by the lambda.
                var stringBuilder = new StringBuilder();

                #region --- Processor Code ---

                return (value) =>
                {
                    // check that the passed value is not null.
                    if (value != null)
                    {
                        stringBuilder.Clear();
                        stringBuilder.Append(name);
                        for (int i = 0; i < value.Count; i++)
                        {
                            stringBuilder.Append(Environment.NewLine);
                            stringBuilder.Append(DEFAULT_INDENT);
                            stringBuilder.Append(listDelegate(value[i], i));
                        }
                        return stringBuilder.ToString();
                    }

                    return nullString;
                };

                #endregion 
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
            }

            return null;
        }

        #endregion
        
        //--------------------------------------------------------------------------------------------------------------

        #region --- Value Processor: Ilist Without Index Argument ---

        private static readonly MethodInfo genericIListWithoutIndexCreateMethod =
            typeof(ValueProcessorFactory).GetMethod(nameof(CreateIListFuncWithoutIndexArgument), STATIC_FLAGS);
        
        private static Func<TInput, string> CreateIListFuncWithoutIndexArgument<TInput, TElement>(MethodInfo processor, string name) where TInput : IList<TElement>
        {
            try
            {
                // create a matching delegate with the processors method info.
                var listDelegate = (Func<TElement, string>)Delegate.CreateDelegate(typeof(Func<TElement, string>), processor);
                
                // create a matching null string.
                var nullString = $"{name}: {NULL}";

                // create a stringBuilder object to be used by the lambda.
                var stringBuilder = new StringBuilder();

                #region --- Processor Code ---

                return (value) =>
                {
                    // check that the passed value is not null.
                    if (value != null)
                    {
                        stringBuilder.Clear();
                        stringBuilder.Append(name);
                        
                        for (int i = 0; i < value.Count; i++)
                        {
                            stringBuilder.Append(Environment.NewLine);
                            stringBuilder.Append(DEFAULT_INDENT);
                            stringBuilder.Append(listDelegate(value[i]));
                        }
                        return stringBuilder.ToString();
                    }

                    return nullString;
                };

                #endregion 
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
            }

            return null;
        }

        #endregion
        
        //--------------------------------------------------------------------------------------------------------------
        
        #region --- Value Processor: Dictionary ---

        
        private static readonly MethodInfo genericIDictionaryCreateMethod =
            typeof(ValueProcessorFactory).GetMethod(nameof(CreateIDictionaryFunc), STATIC_FLAGS);

        private static Func<TInput, string> CreateIDictionaryFunc<TInput, TKey, TValue>(MethodInfo processor, string name) where TInput : IDictionary<TKey, TValue>
        {
            try
            {
                // create a matching delegate with the processors method info.
                var genericFunc = (Func<TKey,TValue,string>)Delegate.CreateDelegate(typeof(Func<TKey,TValue,string>), processor);

                // create a matching null string.
                var nullString = $"{name}: {NULL}";
                
                // create a stringBuilder object to be used by the lambda.
                var stringBuilder = new StringBuilder();
                
                #region --- Processor Code ---

                return (value) =>
                {
                    // check that the passed value is not null.
                    if (value != null)
                    {
                        stringBuilder.Clear();
                        stringBuilder.Append(name);
                        
                        foreach (KeyValuePair<TKey, TValue> valuePair in value)
                        {
                            stringBuilder.Append(Environment.NewLine);
                            stringBuilder.Append(DEFAULT_INDENT);
                            stringBuilder.Append(genericFunc(valuePair.Key, valuePair.Value));
                        }
                        return stringBuilder.ToString();
                    }

                    return nullString;
                };

                #endregion
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
            }

            return null;
        }

        #endregion
    }
}
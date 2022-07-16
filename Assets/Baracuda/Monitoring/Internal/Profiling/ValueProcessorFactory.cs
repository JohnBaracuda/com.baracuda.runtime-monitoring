// Copyright (c) 2022 Jonathan Lang
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
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
        /*
         * API   
         */

        /// <summary>
        /// Creates a default type specific processor to format the <see cref="TValue"/> depending on its exact type.
        /// </summary>
        /// <typeparam name="TValue">The type of the value that should be parsed/formatted</typeparam>
        /// <returns></returns>
        internal static Func<TValue, string> CreateProcessorForType<TValue>(IFormatData formatData)
        {
            return CreateTypeSpecificProcessorInternal<TValue>(formatData);
        }

        //--------------------------------------------------------------------------------------------------------------

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Func<TValue, string> CreateTypeSpecificProcessorInternal<TValue>(IFormatData formatData)
        {
            var type = typeof(TValue);
            
            // Transform
            if (type == typeof(Transform))
            {
                return (Func<TValue, string>)(Delegate) TransformProcessor(formatData);
            }

            // Boolean
            if (type == typeof(bool))
            {
                return (Func<TValue, string>)(Delegate) CreateBooleanProcessor(formatData);
            }
            
            // Dictionary<TKey, TValue>
            if (type.IsGenericIDictionary())
            {
                try
                {
                    var keyType = type.GetGenericArguments()[0];
                    var valueType = type.GetGenericArguments()[1];
                    var genericMethod = createDictionaryProcessorMethod.MakeGenericMethod(keyType, valueType);
                    return (Func<TValue, string>) genericMethod.Invoke(null, new object[] {formatData});
                }
#pragma warning disable CS0618
                //IL2CPP runtime does throw this exception!
                catch (ExecutionEngineException engineException)
#pragma warning restore CS0618
                {
                    Debug.LogException(engineException);
                }
            }
            
            //TODO: Bool array??
            
            // IEnumerable<bool>
            if (type.HasInterface<IEnumerable<bool>>())
            {
                return (Func<TValue, string>) (Delegate) IEnumerableBooleanProcessor(formatData);
            }
            
            // Array<T>
            if (type.IsArray)
            {
                try
                {
                    var elementType = type.GetElementType();
                
                    Debug.Assert(elementType != null, nameof(elementType) + " != null");
                
                    var genericMethod = elementType.IsValueType ? createValueTypeArrayMethod.MakeGenericMethod(elementType) : createReferenceTypeArrayMethod.MakeGenericMethod(elementType);
                
                    return (Func<TValue, string>) genericMethod.Invoke(null, new object[]{formatData});
                }
#pragma warning disable CS0618 
                //IL2CPP runtime does throw this exception!
                catch (ExecutionEngineException engineException)
#pragma warning restore CS0618
                {
                    Debug.LogException(engineException);
                }
            }

            // IEnumerable<T>
            if (type.IsGenericIEnumerable(true))
            {
                try
                {
                    var elementType = type.GetElementType() ?? type.GetGenericArguments()[0];
                    var genericMethod = createGenericIEnumerableMethod.MakeGenericMethod(elementType);
                    return (Func<TValue, string>) genericMethod.Invoke(null, new object[]{formatData});
                }
#pragma warning disable CS0618 
                //IL2CPP runtime does throw this exception!
                catch (ExecutionEngineException engineException)
#pragma warning restore CS0618
                {
                    Debug.LogException(engineException);
                }
            }
            
            // IEnumerable
            if (type.IsIEnumerable(true))
            {
                return (Func<TValue, string>) (Delegate) IEnumerableProcessor(formatData, type);
            }

            // Quaternion
            if (type == typeof(Quaternion))
            {
                return (Func<TValue, string>) (Delegate) QuaternionProcessor(formatData);
            }

            // Vector3
            if (type == typeof(Vector3))
            {
                return (Func<TValue, string>) (Delegate) Vector3Processor(formatData);
            }
            
            // Vector2
            if (type == typeof(Vector2))
            {
                return (Func<TValue, string>) (Delegate) Vector2Processor(formatData);
            }

            // Color
            if (type == typeof(Color))
            {
                return (Func<TValue, string>) (Delegate) ColorProcessor(formatData);
            }
            
            // Color32
            if (type == typeof(Color32))
            {
                return (Func<TValue, string>) (Delegate) Color32Processor(formatData);
            }

            // Format
            if (type.HasInterface<IFormattable>() && formatData.Format != null)
            {
                return FormattedProcessor<TValue>(formatData);
            }
            
            // UnityEngine.Object
            if (type.IsSubclassOf(typeof(UnityEngine.Object)))
            {
                return (Func<TValue, string>) (Delegate) UnityEngineObjectProcessor(formatData);
            }
            
            // Int32
            if (type.IsInt32())
            {
                return (Func<TValue, string>) (Delegate) Int32Processor(formatData);
            }
            
            // Int64
            if (type.IsInt64())
            {
                return (Func<TValue, string>) (Delegate) Int64Processor(formatData);
            }
            
            // Float
            if (type.IsSingle())
            {
                return (Func<TValue, string>) (Delegate) SingleProcessor(formatData);
            }
            
            // Double
            if (type.IsDouble())
            {
                return (Func<TValue, string>) (Delegate) DoubleProcessor(formatData);
            }

            // Value Type
            if (type.IsValueType)
            {
                return ValueTypeProcessor<TValue>(formatData);
            }
            
            // Reference Type
            else
            {
                return DefaultProcessor<TValue>(formatData);
            }
        }
    }
}
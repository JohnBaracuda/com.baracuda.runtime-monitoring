// Copyright (c) 2022 Jonathan Lang

using Baracuda.Monitoring.Utilities.Extensions;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Baracuda.Monitoring.Systems
{
    /// <summary>
    /// Class creates custom ValueProcessor delegates for Monitoring units based on their values type.
    /// </summary>
    internal partial class ValueProcessorFactory
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private Func<TValue, string> CreateTypeSpecificProcessorInternal<TValue>(IFormatData formatData)
        {
            try
            {
                var type = typeof(TValue);

                // Global predefined value processor for type
                if (_globalValueProcessors.TryGetValue(type, out var globalProcessor)
                    && globalProcessor is Func<IFormatData, TValue, string> typeSpecificGlobal)
                {
                    return (value) => typeSpecificGlobal(formatData, value);
                }

                if (type == typeof(Transform))
                {
                    return (Func<TValue, string>) (Delegate) TransformProcessor(formatData);
                }

                if (type == typeof(bool))
                {
                    return (Func<TValue, string>) (Delegate) CreateBooleanProcessor(formatData);
                }

                if (type == typeof(bool[]))
                {
                    return (Func<TValue, string>) (Delegate) BooleanArrayProcessor(formatData);
                }

                // Dictionary<TKey, TValue>
                if (type.IsGenericIDictionary() && !type.IsValueType)
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
                        Debug.LogWarning(engineException);
                    }
                }

                // IEnumerable<bool>
                if (type.HasInterface<IEnumerable<bool>>() && !type.IsValueType)
                {
                    return (Func<TValue, string>) (Delegate) EnumerableBooleanProcessor(formatData);
                }

                if (type.IsArray)
                {
                    try
                    {
                        var elementType = type.GetElementType();

                        Debug.Assert(elementType != null, nameof(elementType) + " != null");

                        var genericMethod = elementType.IsValueType ? createValueTypeArrayMethod.MakeGenericMethod(elementType) : createReferenceTypeArrayMethod.MakeGenericMethod(elementType);

                        return (Func<TValue, string>) genericMethod.Invoke(null, new object[] {formatData});
                    }
#pragma warning disable CS0618
                    //IL2CPP runtime does throw this exception!
                    catch (ExecutionEngineException engineException)
#pragma warning restore CS0618
                    {
                        Debug.LogWarning(engineException);
                    }
                }

                // IEnumerable<T>
                if (type.IsGenericIEnumerable(out var element) && !type.IsValueType)
                {
                    try
                    {
                        var genericMethod = createGenericIEnumerableMethod.MakeGenericMethod(element);
                        return (Func<TValue, string>) genericMethod.Invoke(null, new object[] {formatData});
                    }
#pragma warning disable CS0618
                    //IL2CPP runtime does throw this exception!
                    catch (ExecutionEngineException engineException)
#pragma warning restore CS0618
                    {
                        Debug.LogWarning(engineException);
                    }
                }

                if (type.IsIEnumerable(true) && !type.IsValueType)
                {
                    return (Func<TValue, string>) (Delegate) EnumerableProcessor(formatData, type);
                }

                if (type == typeof(Quaternion))
                {
                    return (Func<TValue, string>) (Delegate) QuaternionProcessor(formatData);
                }

                if (type == typeof(Vector3))
                {
                    return (Func<TValue, string>) (Delegate) Vector3Processor(formatData);
                }

                if (type == typeof(Vector2))
                {
                    return (Func<TValue, string>) (Delegate) Vector2Processor(formatData);
                }

                if (type == typeof(Color))
                {
                    return (Func<TValue, string>) (Delegate) ColorProcessor(formatData);
                }

                if (type == typeof(Color32))
                {
                    return (Func<TValue, string>) (Delegate) Color32Processor(formatData);
                }

                if (type.HasInterface<IFormattable>() && formatData.Format != null)
                {
                    return FormattedProcessor<TValue>(formatData);
                }

                if (type.IsSubclassOf(typeof(Object)))
                {
                    return (Func<TValue, string>) (Delegate) UnityEngineObjectProcessor(formatData);
                }

                if (type.IsInt32())
                {
                    return (Func<TValue, string>) (Delegate) Int32Processor(formatData);
                }

                if (type.IsInt64())
                {
                    return (Func<TValue, string>) (Delegate) Int64Processor(formatData);
                }

                if (type.IsSingle())
                {
                    return (Func<TValue, string>) (Delegate) SingleProcessor(formatData);
                }

                if (type.IsDouble())
                {
                    return (Func<TValue, string>) (Delegate) DoubleProcessor(formatData);
                }

                if (type.IsValueType)
                {
                    return ValueTypeProcessor<TValue>(formatData);
                }
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"[Monitoring] Exception during ValueProcessor creation: {exception}");
            }

            // Everything else that is a reference type.
            return DefaultProcessor<TValue>(formatData);
        }
    }
}
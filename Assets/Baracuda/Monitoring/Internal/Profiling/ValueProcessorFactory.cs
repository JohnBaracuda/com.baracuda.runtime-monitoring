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
        /*
         * API   
         */

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
        
        //--------------------------------------------------------------------------------------------------------------

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
            
            // Dictionary<TKey, TValue>
            if (profile.UnitValueType.IsGenericIDictionary())
            {
                try
                {
                    var keyType = profile.UnitValueType.GetGenericArguments()[0];
                    var valueType = profile.UnitValueType.GetGenericArguments()[1];
                    var genericMethod = createDictionaryProcessorMethod.MakeGenericMethod(keyType, valueType);
                    return (Func<TValue, string>) genericMethod.Invoke(null, new object[] {profile});
                }
#pragma warning disable CS0618
                //IL2CPP runtime does throw this exception!
                catch (ExecutionEngineException engineException)
#pragma warning restore CS0618
                {
                    Debug.LogException(engineException);
                }
            }
            
            // Array<T>
            if (profile.UnitValueType.IsArray)
            {
                try
                {
                    var type = profile.UnitValueType.GetElementType();
                
                    Debug.Assert(type != null, nameof(type) + " != null");
                
                    var genericMethod = type.IsValueType ? createValueTypeArrayMethod.MakeGenericMethod(type) : createReferenceTypeArrayMethod.MakeGenericMethod(type);
                
                    return (Func<TValue, string>) genericMethod.Invoke(null, new object[]{profile});
                }
#pragma warning disable CS0618 
                //IL2CPP runtime does throw this exception!
                catch (ExecutionEngineException engineException)
#pragma warning restore CS0618
                {
                    Debug.LogException(engineException);
                }
            } 
            
            // IEnumerable<bool>
            if (profile.UnitValueType.HasInterface<IEnumerable<bool>>())
            {
                return (Func<TValue, string>) (Delegate) IEnumerableBooleanProcessor(profile);
            }
            
            // IEnumerable<T>
            if (profile.UnitValueType.IsGenericIEnumerable(true))
            {
                try
                {
                    var type = profile.UnitValueType.GetElementType() ?? profile.UnitValueType.GetGenericArguments()[0];
                    var genericMethod = createGenericIEnumerableMethod.MakeGenericMethod(type);
                    return (Func<TValue, string>) genericMethod.Invoke(null, new object[]{profile});
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
                return DefaultProcessor<TValue>(profile);
            }
        }
    }
}
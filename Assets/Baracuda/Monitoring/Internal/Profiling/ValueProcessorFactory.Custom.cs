using System;
using System.Collections.Generic;
using System.Linq;
using Baracuda.Monitoring.Internal.Exceptions;
using Baracuda.Monitoring.Internal.Units;
using Baracuda.Monitoring.Internal.Utilities;
using Baracuda.Reflection;
using UnityEngine;

namespace Baracuda.Monitoring.Internal.Profiling
{
    internal static partial class ValueProcessorFactory
    {
        /*
         * Static   
         */

        /// <summary>
        /// This method will scan the declaring <see cref="Type"/> of the passed
        /// <see cref="ValueProfile{TTarget,TValue}"/> for a valid processor method with the passed name.<br/>
        /// Certain types offer special functionality and require additional handling. Those types are:<br/>
        /// Types assignable from <see cref="IList{T}"/> (inc. <see cref="Array"/>)<br/>
        /// Types assignable from <see cref="IDictionary{TKey, TValue}"/><br/>
        /// Types assignable from <see cref="IEnumerable{T}"/>
        /// </summary>
        /// <param name="processor">name of the method declared as a value processor</param>
        /// <param name="valueProfile">the profile of the <see cref="ValueUnit{TTarget,TValue}"/></param>
        /// <typeparam name="TTarget">the <see cref="Type"/> of the profiles Target instance</typeparam>
        /// <typeparam name="TValue">the <see cref="Type"/> of the profiles value instance</typeparam>
        /// <returns></returns>
        internal static Func<TValue, string> FindCustomStaticProcessor<TTarget, TValue>(
            string processor,
            ValueProfile<TTarget, TValue> valueProfile) where TTarget : class
        {
            try
            {
                // validate that the processor name is not null.
                if (string.IsNullOrWhiteSpace(processor))
                {
                    return null;
                }

                // setup
                var declaringType = valueProfile.UnitTargetType;
                var valueType = valueProfile.UnitValueType;

                // get the processor method.
                var processorMethod = declaringType.GetMethod(processor, STATIC_FLAGS);

                // validate that the processor is not null.
                if (processorMethod == null)
                {
                    ExceptionLogging.LogException(new ProcessorNotFoundException(processor, declaringType));
                    return null;
                }

                // create a delegate with for the processors method.
                var processorFunc = processorMethod.CreateMatchingDelegate(FLAGS);

                //----------------------------

                // cache the parameter information of the processor method.
                var parameterInfos = processorFunc.Method.GetParameters();

                if (!parameterInfos.Any())
                {
                    ExceptionLogging.LogException(new InvalidProcessorSignatureException(processor, declaringType));
                    return null;
                }

                //----------------------------

                // Check if the types are a perfect match
                if (parameterInfos.Length == 1 && parameterInfos[0].ParameterType.IsAssignableFrom(valueType))
                {
                    var defaultDelegate = (Func<TValue, string>) processorFunc;
                    return value => defaultDelegate(value);
                }

                //----------------------------

                #region --- Ilist<T> ---

                // IList<T> processor
                if (valueType.IsGenericIList())
                {
                    //Debug.Log("IList");
                    if (parameterInfos[0].ParameterType.IsAssignableFrom(valueType.IsArray
                            ? valueType.GetElementType()
                            : valueType.GetGenericArguments()[0]))
                    {
                        // IList<T> processor passing each element of the collection
                        if (parameterInfos.Length == 1)
                        {
                            // create a generic method to create the generic processor
                            var delegateCreationMethod = genericIListWithoutIndexCreateMethod
                                .MakeGenericMethod(valueType,
                                    valueType.IsArray
                                        ? valueType.GetElementType()
                                        : valueType.GetGenericArguments()[0]);

                            // create a delegate of type: <Func<TValue, string>> by invoking the delegateCreationMethod
                            var func = delegateCreationMethod
                                .Invoke(null, new object[] {processorFunc.Method, valueProfile.FormatData.Label})
                                ?.ConvertFast<object, Func<TValue, string>>();

                            // return the delegate if it was created successfully
                            if (func != null)
                            {
                                return func;
                            }
                        }

                        // IList<T> processor passing each element of the collection with the associated index.
                        if (parameterInfos.Length == 2 && parameterInfos[1].ParameterType == typeof(int))
                        {
                            // create a generic method to create the generic processor
                            var delegateCreationMethod = genericIListWithIndexCreateMethod
                                .MakeGenericMethod(valueType,
                                    valueType.IsArray
                                        ? valueType.GetElementType()
                                        : valueType.GetGenericArguments()[0]);

                            // create a delegate of type: <Func<TValue, string>> by invoking the delegateCreationMethod
                            var func = delegateCreationMethod
                                .Invoke(null, new object[] {processorFunc.Method, valueProfile.FormatData.Label})
                                ?.ConvertFast<object, Func<TValue, string>>();

                            // return the delegate if it was created successfully
                            if (func != null)
                            {
                                return func;
                            }
                        }
                    }
                }

                #endregion

                //----------------------------

                #region --- IDictionary<TKey,TValue> ---

                // IDictionary<TKey, TValue> processor
                if (valueType.IsGenericIDictionary())
                {
                    // Signature validation 1.
                    // check that the length of the signature is 2, so that it could potentially contain two
                    // valid arguments. (TKey and TValue)
                    if (parameterInfos.Length == 2)
                    {
                        // Signature validation 2.
                        // check that the signature of the processor method is compatible with the generic type definition
                        // of the dictionaries KeyValuePair<TKey, TValue> 
                        var genericArgs = valueType.GetGenericArguments();
                        if (parameterInfos[0].ParameterType == genericArgs[0]
                            && parameterInfos[1].ParameterType == genericArgs[1])
                        {
                            // create a generic method to create the generic processor
                            var delegateCreationMethod = genericIDictionaryCreateMethod
                                .MakeGenericMethod(valueType, genericArgs[0], genericArgs[1]);

                            // create a delegate of type: <Func<TValue, string>> by invoking the delegateCreationMethod
                            var func = delegateCreationMethod
                                .Invoke(null, new object[] {processorFunc.Method, valueProfile.FormatData.Label})
                                ?.ConvertFast<object, Func<TValue, string>>();

                            // return the delegate if it was created successfully
                            if (func != null)
                            {
                                return func;
                            }
                        }
                    }
                }

                #endregion

                //----------------------------

                #region --- IEnumerable<T> ---

                // IEnumerable<T> processor
                if (valueType.IsGenericIEnumerable(true))
                {
                    // check that the signature of the processor method is compatible with the generic type definition
                    // of the IEnumerable<T>s generic type definition.
                    if (parameterInfos.Length == 1 &&
                        parameterInfos[0].ParameterType == valueType.GetGenericArguments()[0])
                    {
                        // create a generic method to create the generic processor
                        var delegateCreationMethod = genericIEnumerableProcessorMethod
                            .MakeGenericMethod(valueType, valueType.GetGenericArguments()[0]);

                        // create a delegate of type: <Func<TValue, string>> by invoking the delegateCreationMethod
                        var func = delegateCreationMethod
                            .Invoke(null, new object[] {processorFunc.Method, valueProfile.FormatData.Label})
                            ?.ConvertFast<object, Func<TValue, string>>();

                        // return the delegate if it was created successfully
                        if (func != null)
                        {
                            return func;
                        }
                    }
                }

                #endregion

                //----------------------------

                return null;
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                return null;
            }
        }
        

        /*
         * Instance   
         */

        /// <summary>
        /// This method will scan the declaring <see cref="Type"/> of the passed
        /// <see cref="ValueProfile{TTarget,TValue}"/> for a valid processor method with the passed name.<br/>
        /// Certain types offer special functionality and require additional handling. Those types are:<br/>
        /// Types assignable from <see cref="IList{T}"/> (inc. <see cref="Array"/>)<br/>
        /// Types assignable from <see cref="IDictionary{TKey, TValue}"/><br/>
        /// Types assignable from <see cref="IEnumerable{T}"/>
        /// </summary>
        /// <param name="processor">name of the method declared as a value processor</param>
        /// <param name="valueProfile">the profile of the <see cref="ValueUnit{TTarget,TValue}"/></param>
        /// <typeparam name="TTarget">the <see cref="Type"/> of the profiles Target instance</typeparam>
        /// <typeparam name="TValue">the <see cref="Type"/> of the profiles value instance</typeparam>
        /// <returns></returns>
        internal static Func<TTarget, TValue, string> FindCustomInstanceProcessor<TTarget, TValue>(
            string processor, ValueProfile<TTarget, TValue> valueProfile) where TTarget : class
        {
            try
            {
                // validate that the processor name is not null.
                if (string.IsNullOrWhiteSpace(processor))
                {
                    return null;
                }

                var declaringType = valueProfile.UnitTargetType;
                var valueType = valueProfile.UnitValueType;

                var processorMethod = declaringType.GetMethod(processor, INSTANCE_FLAGS);

                if (processorMethod == null)
                {
                    ExceptionLogging.LogException(new ProcessorNotFoundException(processor, declaringType));
                    return null;
                }

                // create a delegate with for the processors method.
                var processorFunc =
                    (Func<TTarget, TValue, string>) processorMethod.CreateDelegate(
                        typeof(Func<TTarget, TValue, string>));

                //----------------------------

                // cache the parameter information of the processor method.
                var parameterInfos = processorFunc.Method.GetParameters();

                if (!parameterInfos.Any())
                {
                    ExceptionLogging.LogException(new InvalidProcessorSignatureException(processor, declaringType));
                    return null;
                }

                //----------------------------

                // Check if the types are a perfect match
                if (parameterInfos.Length == 1 && parameterInfos[0].ParameterType.IsAssignableFrom(valueType))
                {
                    return (target, value) => processorFunc(target, value);
                }

                //----------------------------

                return null;
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                return null;
            }
        }
    }
}
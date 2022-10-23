// Copyright (c) 2022 Jonathan Lang

using Baracuda.Monitoring.Profiles;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Baracuda.Monitoring.Systems
{
    internal partial class ValueProcessorFactory
    {
        /// <summary>
        /// Creates a default type specific processor to format the value depending on its exact type.
        /// </summary>
        /// <typeparam name="TValue">The type of the value that should be parsed/formatted</typeparam>
        /// <returns></returns>
        public Func<TValue, string> CreateProcessorForType<TValue>(IFormatData formatData)
        {
            return CreateTypeSpecificProcessorInternal<TValue>(formatData);
        }

        /// <summary>
        /// This method will scan the declaring <see cref="Type"/> of the passed
        /// <see cref="ValueProfile{TTarget,TValue}"/> for a valid processor method with the passed name.<br/>
        /// Certain types offer special functionality and require additional handling. Those types are:<br/>
        /// Types assignable from <see cref="IList{T}"/> (inc. <see cref="Array"/>)<br/>
        /// Types assignable from <see cref="IDictionary{TKey, TValue}"/><br/>
        /// Types assignable from <see cref="IEnumerable{T}"/>
        /// </summary>
        /// <param name="processor">name of the method declared as a value processor</param>
        /// <param name="formatData"></param>
        /// <typeparam name="TTarget">the <see cref="Type"/> of the profiles Target instance</typeparam>
        /// <typeparam name="TValue">the <see cref="Type"/> of the profiles value instance</typeparam>
        /// <returns></returns>
        public Func<TValue, string> FindCustomStaticProcessor<TTarget, TValue>(
            string processor,
            IFormatData formatData) where TTarget : class
        {
            return FindCustomStaticProcessorInternal<TTarget, TValue>(processor, formatData);
        }

        /// <summary>
        /// This method will scan the declaring <see cref="Type"/> of the passed
        /// <see cref="ValueProfile{TTarget,TValue}"/> for a valid processor method with the passed name.<br/>
        /// Certain types offer special functionality and require additional handling. Those types are:<br/>
        /// Types assignable from <see cref="IList{T}"/> (inc. <see cref="Array"/>)<br/>
        /// Types assignable from <see cref="IDictionary{TKey, TValue}"/><br/>
        /// Types assignable from <see cref="IEnumerable{T}"/>
        /// </summary>
        /// <param name="processor">name of the method declared as a value processor</param>
        /// <param name="formatData"></param>
        /// <typeparam name="TTarget">the <see cref="Type"/> of the profiles Target instance</typeparam>
        /// <typeparam name="TValue">the <see cref="Type"/> of the profiles value instance</typeparam>
        /// <returns></returns>
        public Func<TTarget, TValue, string> FindCustomInstanceProcessor<TTarget, TValue>(
            string processor, IFormatData formatData) where TTarget : class
        {
            return FindCustomInstanceProcessorInternal<TTarget, TValue>(processor, formatData);
        }

        /// <summary>
        /// Add a global value processor.
        /// </summary>
        /// <param name="methodInfo"></param>
        public void AddGlobalValueProcessor(MethodInfo methodInfo)
        {
            AddGlobalValueProcessorInternal(methodInfo);
        }
    }
}
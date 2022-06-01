using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace Baracuda.Monitoring.Internal.Profiling
{
    internal static partial class ValueProcessorFactory
    {
        /*
         * Value Processor IEnumerable   
         */

        private static readonly MethodInfo genericIEnumerableProcessorMethod =
            typeof(ValueProcessorFactory).GetMethod(nameof(CreateIEnumerableFunc), STATIC_FLAGS);

        private static Func<TInput, string> CreateIEnumerableFunc<TInput, TElement>(MethodInfo processor, string name)
            where TInput : IEnumerable<TElement>
        {
            try
            {
                // create a matching delegate with the processors method info.
                var enumerableDelegate =
                    (Func<TElement, string>) Delegate.CreateDelegate(typeof(Func<TElement, string>), processor);

                // create a matching null string.
                var nullString = $"{name}: {NULL}";

                // create a stringBuilder object to be used by the lambda.
                var sb = new StringBuilder();

                //Processor Code
                return (value) =>
                {
                    // check that the passed value is not null.
                    if ((object) value != null)
                    {
                        sb.Clear();
                        sb.Append(name);
                        foreach (TElement element in value)
                        {
                            sb.Append(Environment.NewLine);
                            sb.Append(DEFAULT_INDENT);
                            sb.Append(enumerableDelegate(element));
                        }

                        return sb.ToString();
                    }

                    return nullString;
                };
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
            }

            return null;
        }

    }
}
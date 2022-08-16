// Copyright (c) 2022 Jonathan Lang

using System;
using System.Linq;
using System.Reflection;
using System.Text;
using Baracuda.Monitoring.API;
using Baracuda.Utilities.Extensions;
using Baracuda.Utilities.Reflection;
using Object = UnityEngine.Object;

namespace Baracuda.Monitoring.Source.Systems
{
    internal partial class ValueProcessorFactory
    {
        #region --- Bool Array ---

        private Func<bool[], string> BooleanArrayProcessor(IFormatData formatData)
        {
            var name = formatData.Label;
            var nullString = $"{name}: {NULL} (bool[])";
            var stringBuilder = new StringBuilder();
            var indent = GetIndentStringForProfile(formatData);

            return formatData.ShowIndexer
                ? (Func<bool[], string>) ((value) =>
                {
                    if (value == null)
                    {
                        return nullString;
                    }

                    var index = 0;

                    stringBuilder.Clear();
                    stringBuilder.Append(name);

                    for (var i = 0; i < value.Length; i++)
                    {
                        var element = value[i];
                        stringBuilder.Append(Environment.NewLine);
                        stringBuilder.Append(indent);
                        stringBuilder.Append('[');
                        stringBuilder.Append(index++);
                        stringBuilder.Append("]: ");
                        stringBuilder.Append(element ? _trueColored : _falseColored);
                    }

                    return stringBuilder.ToString();
                })
                : (value) =>
                {
                    if (value == null)
                    {
                        return nullString;
                    }

                    stringBuilder.Clear();
                    stringBuilder.Append(name);

                    for (var i = 0; i < value.Length; i++)
                    {
                        var element = value[i];
                        stringBuilder.Append(Environment.NewLine);
                        stringBuilder.Append(indent);
                        stringBuilder.Append(element ? _trueColored : _falseColored);
                    }

                    return stringBuilder.ToString();
                };
        }
        
        #endregion
        
        //--------------------------------------------------------------------------------------------------------------
        
        #region --- ReferenceType ---

        private static readonly MethodInfo createReferenceTypeArrayMethod = typeof(ValueProcessorFactory)
            .GetMethods(BindingFlags.Static | BindingFlags.NonPublic)
            .Single(methodInfo =>
                methodInfo.Name == nameof(ReferenceTypeArrayProcessor) && methodInfo.IsGenericMethodDefinition);

        private static Func<T[], string> ReferenceTypeArrayProcessor<T>(IFormatData formatData)
        {
            var name = formatData.Label;
            var nullString = $"{name}: {NULL}";
            var stringBuilder = new StringBuilder();
            var indent = GetIndentStringForProfile(formatData);

            if (typeof(T).IsSubclassOrAssignable(typeof(Object)))
            {
                return formatData.ShowIndexer
                    ? (Func<T[], string>) ((value) =>
                    {
                        if (value == null)
                        {
                            return nullString;
                        }

                        var index = 0;

                        stringBuilder.Clear();
                        stringBuilder.Append(name);

                        foreach (var element in value)
                        {
                            stringBuilder.Append(Environment.NewLine);
                            stringBuilder.Append(indent);
                            stringBuilder.Append('[');
                            stringBuilder.Append(index++);
                            stringBuilder.Append("]: ");
                            stringBuilder.Append(element != null ? element.ToString() : NULL);
                        }

                        return stringBuilder.ToString();
                    })
                    : (value) =>
                    {
                        if (value == null)
                        {
                            return nullString;
                        }

                        stringBuilder.Clear();
                        stringBuilder.Append(name);

                        foreach (var element in value)
                        {
                            stringBuilder.Append(Environment.NewLine);
                            stringBuilder.Append(indent);
                            stringBuilder.Append(element != null ? element.ToString() : NULL);
                        }

                        return stringBuilder.ToString();
                    };
            }
            else
            {
                if (formatData.ShowIndexer)
                {
                    return (value) =>
                    {
                        if (value == null)
                        {
                            return nullString;
                        }

                        var index = 0;

                        stringBuilder.Clear();
                        stringBuilder.Append(name);

                        foreach (var element in value)
                        {
                            stringBuilder.Append(Environment.NewLine);
                            stringBuilder.Append(indent);
                            stringBuilder.Append('[');
                            stringBuilder.Append(index++);
                            stringBuilder.Append("]: ");
                            stringBuilder.Append(element?.ToString() ?? NULL);
                        }

                        return stringBuilder.ToString();
                    };
                }
                else
                {
                    return (value) =>
                    {
                        if (value == null)
                        {
                            return nullString;
                        }

                        stringBuilder.Clear();
                        stringBuilder.Append(name);

                        foreach (var element in value)
                        {
                            stringBuilder.Append(Environment.NewLine);
                            stringBuilder.Append(indent);
                            stringBuilder.Append(element?.ToString() ?? NULL);
                        }

                        return stringBuilder.ToString();
                    };
                }
            }
        }

        #endregion

        #region --- ValueType ---
        
        private static readonly MethodInfo createValueTypeArrayMethod = typeof(ValueProcessorFactory)
            .GetMethods(BindingFlags.Static | BindingFlags.NonPublic)
            .Single(methodInfo =>
                methodInfo.Name == nameof(ValueTypeArrayProcessor) && methodInfo.IsGenericMethodDefinition);

        private static Func<T[], string> ValueTypeArrayProcessor<T>(IFormatData formatData) where T : struct
        {
            var name = formatData.Label;
            var nullString = $"{name}: {NULL}";
            var stringBuilder = new StringBuilder();
            var indent = GetIndentStringForProfile(formatData);

            return formatData.ShowIndexer
                ? (Func<T[], string>) ((value) =>
                {
                    if (value == null)
                    {
                        return nullString;
                    }

                    var index = 0;

                    stringBuilder.Clear();
                    stringBuilder.Append(name);

                    foreach (var element in value)
                    {
                        stringBuilder.Append(Environment.NewLine);
                        stringBuilder.Append(indent);
                        stringBuilder.Append('[');
                        stringBuilder.Append(index++);
                        stringBuilder.Append("]: ");
                        stringBuilder.Append(element.ToString());
                    }

                    return stringBuilder.ToString();
                })
                : (value) =>
                {
                    if (value == null)
                    {
                        return nullString;
                    }

                    stringBuilder.Clear();
                    stringBuilder.Append(name);

                    foreach (var element in value)
                    {
                        stringBuilder.Append(Environment.NewLine);
                        stringBuilder.Append(indent);
                        stringBuilder.Append(element.ToString());
                    }

                    return stringBuilder.ToString();
                };
        }

        #endregion
    }
}
// Copyright (c) 2022 Jonathan Lang

using Baracuda.Monitoring.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Baracuda.Monitoring.Systems
{
    internal partial class ValueProcessorFactory
    {
        private static readonly MethodInfo createDictionaryProcessorMethod = typeof(ValueProcessorFactory)
            .GetMethods(BindingFlags.Static | BindingFlags.NonPublic)
            .Single(methodInfo =>
                methodInfo.Name == nameof(DictionaryProcessor) && methodInfo.IsGenericMethodDefinition);

        private static Func<IDictionary<TKey, TValue>, string> DictionaryProcessor<TKey, TValue>(FormatData formatData)
        {
            var name = formatData.Label;
            var stringBuilder = new StringBuilder();
            var nullString = $"{name}: {Null}";
            var indent = GetIndentStringForProfile(formatData);

            if (typeof(TKey).IsValueType)
            {
                if (typeof(TValue).IsValueType)
                {
                    if (formatData.ShowIndex)
                    {
                        return value =>
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
                                stringBuilder.Append(']');
                                stringBuilder.Append(' ');
                                stringBuilder.Append('[');
                                stringBuilder.Append(element.Key);
                                stringBuilder.Append(',');
                                stringBuilder.Append(' ');
                                stringBuilder.Append(element.Value);
                                stringBuilder.Append(']');
                            }

                            return stringBuilder.ToString();
                        };
                    }
                    return value =>
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
                            stringBuilder.Append(' ');
                            stringBuilder.Append('[');
                            stringBuilder.Append(element.Key);
                            stringBuilder.Append(',');
                            stringBuilder.Append(' ');
                            stringBuilder.Append(element.Value);
                            stringBuilder.Append(']');
                        }

                        return stringBuilder.ToString();
                    };
                }
                if (formatData.ShowIndex)
                {
                    return value =>
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
                            stringBuilder.Append(']');
                            stringBuilder.Append(' ');
                            stringBuilder.Append('[');
                            stringBuilder.Append(element.Key);
                            stringBuilder.Append(',');
                            stringBuilder.Append(' ');
                            stringBuilder.Append(element.Value);
                            stringBuilder.Append(']');
                        }

                        return stringBuilder.ToString();
                    };
                }
                return value =>
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
                        stringBuilder.Append(' ');
                        stringBuilder.Append('[');
                        stringBuilder.Append(element.Key);
                        stringBuilder.Append(',');
                        stringBuilder.Append(' ');
                        stringBuilder.Append(element.Value);
                        stringBuilder.Append(']');
                    }

                    return stringBuilder.ToString();
                };
            }
            if (typeof(TValue).IsValueType)
            {
                if (formatData.ShowIndex)
                {
                    return value =>
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
                            stringBuilder.Append(']');
                            stringBuilder.Append(' ');
                            stringBuilder.Append('[');
                            stringBuilder.Append(element.Key);
                            stringBuilder.Append(',');
                            stringBuilder.Append(' ');
                            stringBuilder.Append(element.Value);
                            stringBuilder.Append(']');
                        }

                        return stringBuilder.ToString();
                    };
                }
                return value =>
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
                        stringBuilder.Append(' ');
                        stringBuilder.Append('[');
                        stringBuilder.Append(element.Key);
                        stringBuilder.Append(',');
                        stringBuilder.Append(' ');
                        stringBuilder.Append(element.Value);
                        stringBuilder.Append(']');
                    }

                    return stringBuilder.ToString();
                };
            }
            if (formatData.ShowIndex)
            {
                return value =>
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
                        stringBuilder.Append(']');
                        stringBuilder.Append(' ');
                        stringBuilder.Append('[');
                        stringBuilder.Append(element.Key);
                        stringBuilder.Append(',');
                        stringBuilder.Append(' ');
                        stringBuilder.Append(element.Value);
                        stringBuilder.Append(']');
                    }

                    return stringBuilder.ToString();
                };
            }
            return value =>
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
                    stringBuilder.Append(' ');
                    stringBuilder.Append('[');
                    stringBuilder.Append(element.Key);
                    stringBuilder.Append(',');
                    stringBuilder.Append(' ');
                    stringBuilder.Append(element.Value);
                    stringBuilder.Append(']');
                }

                return stringBuilder.ToString();
            };
        }
    }
}
// Copyright (c) 2022 Jonathan Lang

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Baracuda.Monitoring.Source.Types;

namespace Baracuda.Monitoring.Source.Systems
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
            var nullString = $"{name}: {NULL}";
            var indent = GetIndentStringForProfile(formatData);

            if (typeof(TKey).IsValueType)
            {
                if (typeof(TValue).IsValueType)
                {
                    return formatData.ShowIndexer
                        ? (Func<IDictionary<TKey, TValue>, string>) ((value) =>
                        {
                            if (value == null)
                            {
                                return nullString;
                            }

                            var index = 0;
                            stringBuilder.Clear();
                            stringBuilder.Append(name);

                            foreach (KeyValuePair<TKey, TValue> element in value)
                            {
                                stringBuilder.Append(Environment.NewLine);
                                stringBuilder.Append(indent);
                                stringBuilder.Append('[');
                                stringBuilder.Append(index++);
                                stringBuilder.Append(']');
                                stringBuilder.Append(' ');
                                stringBuilder.Append('[');
                                stringBuilder.Append(element.Key.ToString());
                                stringBuilder.Append(',');
                                stringBuilder.Append(' ');
                                stringBuilder.Append(element.Value.ToString());
                                stringBuilder.Append(']');
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

                            foreach (KeyValuePair<TKey, TValue> element in value)
                            {
                                stringBuilder.Append(Environment.NewLine);
                                stringBuilder.Append(indent);
                                stringBuilder.Append(' ');
                                stringBuilder.Append('[');
                                stringBuilder.Append(element.Key.ToString());
                                stringBuilder.Append(',');
                                stringBuilder.Append(' ');
                                stringBuilder.Append(element.Value.ToString());
                                stringBuilder.Append(']');
                            }

                            return stringBuilder.ToString();
                        };
                }
                else
                {
                    return formatData.ShowIndexer
                        ? (Func<IDictionary<TKey, TValue>, string>) ((value) =>
                        {
                            if (value == null)
                            {
                                return nullString;
                            }

                            var index = 0;
                            stringBuilder.Clear();
                            stringBuilder.Append(name);

                            foreach (KeyValuePair<TKey, TValue> element in value)
                            {
                                stringBuilder.Append(Environment.NewLine);
                                stringBuilder.Append(indent);
                                stringBuilder.Append('[');
                                stringBuilder.Append(index++);
                                stringBuilder.Append(']');
                                stringBuilder.Append(' ');
                                stringBuilder.Append('[');
                                stringBuilder.Append(element.Key.ToString());
                                stringBuilder.Append(',');
                                stringBuilder.Append(' ');
                                stringBuilder.Append(element.Value?.ToString());
                                stringBuilder.Append(']');
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

                            foreach (KeyValuePair<TKey, TValue> element in value)
                            {
                                stringBuilder.Append(Environment.NewLine);
                                stringBuilder.Append(indent);
                                stringBuilder.Append(' ');
                                stringBuilder.Append('[');
                                stringBuilder.Append(element.Key.ToString());
                                stringBuilder.Append(',');
                                stringBuilder.Append(' ');
                                stringBuilder.Append(element.Value?.ToString());
                                stringBuilder.Append(']');
                            }

                            return stringBuilder.ToString();
                        };
                }
            }
            else
            {
                if (typeof(TValue).IsValueType)
                {
                    return formatData.ShowIndexer
                        ? (Func<IDictionary<TKey, TValue>, string>) ((value) =>
                        {
                            if (value == null)
                            {
                                return nullString;
                            }

                            var index = 0;
                            stringBuilder.Clear();
                            stringBuilder.Append(name);

                            foreach (KeyValuePair<TKey, TValue> element in value)
                            {
                                stringBuilder.Append(Environment.NewLine);
                                stringBuilder.Append(indent);
                                stringBuilder.Append('[');
                                stringBuilder.Append(index++);
                                stringBuilder.Append(']');
                                stringBuilder.Append(' ');
                                stringBuilder.Append('[');
                                stringBuilder.Append(element.Key?.ToString());
                                stringBuilder.Append(',');
                                stringBuilder.Append(' ');
                                stringBuilder.Append(element.Value.ToString());
                                stringBuilder.Append(']');
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

                            foreach (KeyValuePair<TKey, TValue> element in value)
                            {
                                stringBuilder.Append(Environment.NewLine);
                                stringBuilder.Append(indent);
                                stringBuilder.Append(' ');
                                stringBuilder.Append('[');
                                stringBuilder.Append(element.Key?.ToString());
                                stringBuilder.Append(',');
                                stringBuilder.Append(' ');
                                stringBuilder.Append(element.Value.ToString());
                                stringBuilder.Append(']');
                            }

                            return stringBuilder.ToString();
                        };
                }
                else
                {
                    return formatData.ShowIndexer
                        ? (Func<IDictionary<TKey, TValue>, string>) ((value) =>
                        {
                            if (value == null)
                            {
                                return nullString;
                            }

                            var index = 0;
                            stringBuilder.Clear();
                            stringBuilder.Append(name);

                            foreach (KeyValuePair<TKey, TValue> element in value)
                            {
                                stringBuilder.Append(Environment.NewLine);
                                stringBuilder.Append(indent);
                                stringBuilder.Append('[');
                                stringBuilder.Append(index++);
                                stringBuilder.Append(']');
                                stringBuilder.Append(' ');
                                stringBuilder.Append('[');
                                stringBuilder.Append(element.Key?.ToString());
                                stringBuilder.Append(',');
                                stringBuilder.Append(' ');
                                stringBuilder.Append(element.Value?.ToString());
                                stringBuilder.Append(']');
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

                            foreach (KeyValuePair<TKey, TValue> element in value)
                            {
                                stringBuilder.Append(Environment.NewLine);
                                stringBuilder.Append(indent);
                                stringBuilder.Append(' ');
                                stringBuilder.Append('[');
                                stringBuilder.Append(element.Key?.ToString());
                                stringBuilder.Append(',');
                                stringBuilder.Append(' ');
                                stringBuilder.Append(element.Value?.ToString());
                                stringBuilder.Append(']');
                            }

                            return stringBuilder.ToString();
                        };
                }
            }
        }
    }
}
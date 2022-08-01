// Copyright (c) 2022 Jonathan Lang
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace Baracuda.Monitoring.Internal.Profiling
{
    internal static partial class ValueProcessorFactory
    {
        private static readonly MethodInfo createDictionaryProcessorMethod = typeof(ValueProcessorFactory)
            .GetMethods(BindingFlags.Static | BindingFlags.NonPublic)
            .Single(methodInfo =>
                methodInfo.Name == nameof(DictionaryProcessor) && methodInfo.IsGenericMethodDefinition);

        private static Func<IDictionary<TKey, TValue>, string> DictionaryProcessor<TKey, TValue>(MonitorProfile profile)
        {
            var name = profile.FormatData.Label;
            var stringBuilder = new StringBuilder();
            var nullString = $"{name}: {NULL}";
            var indent = GetIndentStringForProfile(profile);

            if (typeof(TKey).IsValueType)
            {
                if (typeof(TValue).IsValueType)
                {
                    return profile.FormatData.ShowIndexer
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
                    return profile.FormatData.ShowIndexer
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
                    return profile.FormatData.ShowIndexer
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
                    return profile.FormatData.ShowIndexer
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
// Copyright (c) 2022 Jonathan Lang

using Baracuda.Monitoring.Utilities.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Object = UnityEngine.Object;

namespace Baracuda.Monitoring.Systems
{
    internal partial class ValueProcessorFactory
    {
        #region IEnumerable

        private static Func<IEnumerable, string> EnumerableProcessor(IFormatData formatData, Type unityValueType)
        {
            var name = formatData.Label;
            var nullString = $"{name}: {Null}";
            var stringBuilder = new StringBuilder();
            var indent = GetIndentStringForProfile(formatData);

            if (unityValueType.IsSubclassOrAssignable(typeof(Object)))
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
                            stringBuilder.Append("]: ");
                            stringBuilder.Append(element);
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
                        stringBuilder.Append(element);
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
                        stringBuilder.Append("]: ");
                        stringBuilder.Append(element);
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
                    stringBuilder.Append(element);
                }

                return stringBuilder.ToString();
            };
        }

        #endregion


        #region Generic IEnumerable

        private static readonly MethodInfo createGenericIEnumerableMethod = typeof(ValueProcessorFactory)
            .GetMethods(BindingFlags.Static | BindingFlags.NonPublic)
            .Single(methodInfo =>
                methodInfo.Name == nameof(GenericIEnumerableProcessor) && methodInfo.IsGenericMethodDefinition);

        private static Func<IEnumerable<T>, string> GenericIEnumerableProcessor<T>(IFormatData formatData)
        {
            var name = formatData.Label;
            var nullString = $"{name}: {Null}";
            var stringBuilder = new StringBuilder();
            var indent = GetIndentStringForProfile(formatData);

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
                        stringBuilder.Append("]: ");
                        if (element == null)
                        {
                            stringBuilder.Append(Null);
                        }
                        else
                        {
                            stringBuilder.Append(element);
                        }
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
                    if (element == null)
                    {
                        stringBuilder.Append(Null);
                    }
                    else
                    {
                        stringBuilder.Append(element);
                    }
                }

                return stringBuilder.ToString();
            };
        }

        private Func<IEnumerable<bool>, string> EnumerableBooleanProcessor(IFormatData formatData)
        {
            var name = formatData.Label;
            var nullString = $"{name}: {Null} (IEnumerable<bool>)";
            var tureString = _trueColored;
            var falseString = _falseColored;
            var stringBuilder = new StringBuilder();
            var indent = GetIndentStringForProfile(formatData);

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
                        stringBuilder.Append("]: ");
                        stringBuilder.Append(element ? tureString : falseString);
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
                    stringBuilder.Append(element ? tureString : falseString);
                }

                return stringBuilder.ToString();
            };
        }

        #endregion
    }
}
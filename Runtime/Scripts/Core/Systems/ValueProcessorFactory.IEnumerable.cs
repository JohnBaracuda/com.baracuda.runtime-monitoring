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
        #region IEnumerable ---

        private static Func<IEnumerable, string> EnumerableProcessor(IFormatData formatData, Type unityValueType)
        {
            var name = formatData.Label;
            var nullString = $"{name}: {NULL}";
            var stringBuilder = new StringBuilder();
            var indent = GetIndentStringForProfile(formatData);

            if (unityValueType.IsSubclassOrAssignable(typeof(UnityEngine.Object)))
            {
                return formatData.ShowIndex
                    ? (Func<IEnumerable, string>) ((value) =>
                    {
                        if ((UnityEngine.Object) value == null)
                        {
                            return nullString;
                        }

                        var index = 0;
                        stringBuilder.Clear();
                        stringBuilder.Append(name);

                        foreach (object element in value)
                        {
                            stringBuilder.Append(Environment.NewLine);
                            stringBuilder.Append(indent);
                            stringBuilder.Append('[');
                            stringBuilder.Append(index++);
                            stringBuilder.Append("]: ");
                            stringBuilder.Append(element);
                        }

                        return stringBuilder.ToString();
                    })
                    : (value) =>
                    {
                        if ((UnityEngine.Object) value == null)
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
            else
            {
                return formatData.ShowIndex
                    ? (Func<IEnumerable, string>) ((value) =>
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
                            stringBuilder.Append(element);
                        }

                        return stringBuilder.ToString();
                    };
            }
        }

        #endregion

        #region --- Generic IEnumerable ---

        private static readonly MethodInfo createGenericIEnumerableMethod = typeof(ValueProcessorFactory)
            .GetMethods(BindingFlags.Static | BindingFlags.NonPublic)
            .Single(methodInfo =>
                methodInfo.Name == nameof(GenericIEnumerableProcessor) && methodInfo.IsGenericMethodDefinition);

        private static Func<IEnumerable<T>, string> GenericIEnumerableProcessor<T>(IFormatData formatData)
        {
            var type = typeof(T);
            var name = formatData.Label;
            var nullString = $"{name}: {NULL}";
            var stringBuilder = new StringBuilder();
            var indent = GetIndentStringForProfile(formatData);

            // Unity objects might not be properly initialized in builds leading to a false result when performing a null check.
#if UNITY_EDITOR
            if (typeof(Object).IsAssignableFrom(type))
            {
                return formatData.ShowIndex
                    ? (Func<IEnumerable<T>, string>) ((value) =>
                    {
                        // ReSharper disable once SuspiciousTypeConversion.Global
                        if ((Object) value == null)
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
                    })
                    : (value) =>
                    {
                        // ReSharper disable once SuspiciousTypeConversion.Global
                        if ((UnityEngine.Object) value == null)
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
            else
#endif //UNITY_EDITOR
            {
                return formatData.ShowIndex
                    ? (Func<IEnumerable<T>, string>) ((value) =>
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
                            stringBuilder.Append(element);
                        }

                        return stringBuilder.ToString();
                    };
            }
        }

        private Func<IEnumerable<bool>, string> EnumerableBooleanProcessor(IFormatData formatData)
        {
            var name = formatData.Label;
            var nullString = $"{name}: {NULL} (IEnumerable<bool>)";
            var tureString = _trueColored;
            var falseString = _falseColored;
            var stringBuilder = new StringBuilder();
            var indent = GetIndentStringForProfile(formatData);

            return formatData.ShowIndex
                ? (Func<IEnumerable<bool>, string>) ((value) =>
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
                        stringBuilder.Append(element ? tureString : falseString);
                    }

                    return stringBuilder.ToString();
                };
        }

        #endregion
    }
}
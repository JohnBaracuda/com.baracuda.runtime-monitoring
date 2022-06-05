// Copyright (c) 2022 Jonathan Lang
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using Baracuda.Monitoring.Internal.Utilities;
using Baracuda.Reflection;
using UnityEngine;
using UnityEngine.Scripting;

namespace Baracuda.Monitoring.Internal.Profiling
{
    internal static partial class ValueProcessorFactory
    {
        #region --- IEnumerable ---

        private static Func<IEnumerable, string> IEnumerableProcessor(MonitorProfile profile)
        {
            var name = profile.FormatData.Label;
            var nullString = $"{name}: {NULL}";
            var stringBuilder = new StringBuilder();
            var indent = GetIndentStringForProfile(profile);

            if (profile.UnitValueType.IsSubclassOrAssignable(typeof(UnityEngine.Object)))
            {
                return profile.FormatData.ShowIndexer
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
                return profile.FormatData.ShowIndexer
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

        private static Func<IEnumerable<T>, string> GenericIEnumerableProcessor<T>(MonitorProfile profile)
        {
            var name = profile.FormatData.Label;
            var nullString = $"{name}: {NULL}";
            var stringBuilder = new StringBuilder();
            var indent = GetIndentStringForProfile(profile);

            // Unity objects might not be properly initialized in builds leading to a false result when performing a null check.
#if UNITY_EDITOR
            if (profile.UnitValueType.IsSubclassOrAssignable(typeof(UnityEngine.Object)))
            {
                return profile.FormatData.ShowIndexer
                    ? (Func<IEnumerable<T>, string>) ((value) =>
                    {
                        // ReSharper disable once SuspiciousTypeConversion.Global
                        if ((UnityEngine.Object) value == null)
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
                return profile.FormatData.ShowIndexer
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

        private static Func<IEnumerable<bool>, string> IEnumerableBooleanProcessor(MonitorProfile profile)
        {
            var name = profile.FormatData.Label;
            var nullString = $"{name}: {NULL} (IEnumerable<bool>)";
            var tureString = $"<color=green>TRUE</color>";
            var falseString = $"<color=red>FALSE</color>";
            var stringBuilder = new StringBuilder();
            var indent = GetIndentStringForProfile(profile);

            return profile.FormatData.ShowIndexer
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
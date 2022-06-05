// Copyright (c) 2022 Jonathan Lang
using System;
using System.Linq;
using System.Reflection;
using System.Text;
using Baracuda.Reflection;
using Object = UnityEngine.Object;

namespace Baracuda.Monitoring.Internal.Profiling
{
    internal static partial class ValueProcessorFactory
    {
        #region --- ReferenceType ---

        private static readonly MethodInfo createReferenceTypeArrayMethod = typeof(ValueProcessorFactory)
            .GetMethods(BindingFlags.Static | BindingFlags.NonPublic)
            .Single(methodInfo =>
                methodInfo.Name == nameof(ReferenceTypeArrayProcessor) && methodInfo.IsGenericMethodDefinition);

        private static Func<T[], string> ReferenceTypeArrayProcessor<T>(MonitorProfile profile)
        {
            var name = profile.FormatData.Label;
            var nullString = $"{name}: {NULL}";
            var stringBuilder = new StringBuilder();
            var indent = GetIndentStringForProfile(profile);

            if (typeof(T).IsSubclassOrAssignable(typeof(Object)))
            {
                return profile.FormatData.ShowIndexer
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
                if (profile.FormatData.ShowIndexer)
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

        private static Func<T[], string> ValueTypeArrayProcessor<T>(MonitorProfile profile) where T : unmanaged
        {
            var name = profile.FormatData.Label;
            var nullString = $"{name}: {NULL}";
            var stringBuilder = new StringBuilder();
            var indent = GetIndentStringForProfile(profile);

            return profile.FormatData.ShowIndexer
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
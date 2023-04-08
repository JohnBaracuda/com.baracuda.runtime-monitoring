// Copyright (c) 2022 Jonathan Lang

using System;
using System.Text;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Baracuda.Monitoring.Systems
{
    internal partial class ValueProcessorFactory
    {
        /*
         * General
         */

        private static Func<TValue, string> DefaultProcessor<TValue>(IFormatData formatData)
        {
            var stringBuilder = new StringBuilder();
            var label = formatData.Label;
            return value =>
            {
                stringBuilder.Clear();
                stringBuilder.Append(label);
                stringBuilder.Append(": ");
                stringBuilder.Append(value?.ToString() ?? Null);
                return stringBuilder.ToString();
            };
        }

        /*
         * Formatted
         */

        private static Func<TValue, string> FormattedProcessor<TValue>(IFormatData formatData)
        {
            var stringBuilder = new StringBuilder();
            var label = formatData.Label;
            var format = formatData.Format;
            return value =>
            {
                stringBuilder.Clear();
                stringBuilder.Append(label);
                stringBuilder.Append(": ");
                stringBuilder.Append((value as IFormattable)?.ToString(format, null) ?? Null);
                return stringBuilder.ToString();
            };
        }

        /*
         * Unity Objects
         */

        private static Func<Object, string> UnityEngineObjectProcessor(IFormatData formatData)
        {
            var name = formatData.Label;
            var stringBuilder = new StringBuilder();

            return value =>
            {
                stringBuilder.Clear();
                stringBuilder.Append(name);
                stringBuilder.Append(": ");
                stringBuilder.Append(value != null ? value.ToString() : Null);
                return stringBuilder.ToString();
            };
        }

        private static Func<Transform, string> TransformProcessor(IFormatData formatData)
        {
            var sb = new StringBuilder();
            var name = formatData.Label;
            var nullString = $"{name}: {Null}";
            var indentValue = CreateIndentValueForProfile(formatData) * 2;
            var cachedString = default(string);

            return transform =>
            {
                if (transform == null)
                {
                    return nullString;
                }

                if (!transform.hasChanged && cachedString != null)
                {
                    return cachedString;
                }

                sb.Clear();
                sb.Append(name);
                sb.Append(":\n-");
                sb.Append(' ');
                sb.Append(transform.name);

                foreach (Transform element in transform)
                {
                    sb.Append("\n-");
                    sb.Append(GetIndentStringForValue(indentValue));
                    sb.Append('-');
                    sb.Append(' ');
                    sb.Append(element.name);

                    foreach (Transform child in element)
                    {
                        Traverse(child, 1, ref sb);
                    }
                }

                cachedString = sb.ToString();
                return cachedString;

                void Traverse(Transform parent, int i, ref StringBuilder builder)
                {
                    builder.Append("\n-");
                    builder.Append(GetIndentStringForValue(++i * indentValue));
                    builder.Append('-');
                    builder.Append(' ');
                    builder.Append(parent.name);
                    foreach (Transform child in parent)
                    {
                        Traverse(child, i, ref builder);
                    }
                }
            };
        }
    }
}
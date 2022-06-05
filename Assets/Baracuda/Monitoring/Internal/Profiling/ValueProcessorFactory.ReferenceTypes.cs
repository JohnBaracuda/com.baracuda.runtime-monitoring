// Copyright (c) 2022 Jonathan Lang
using System;
using System.Text;
using UnityEngine;

namespace Baracuda.Monitoring.Internal.Profiling
{
    internal static partial class ValueProcessorFactory
    {
        /*
         * General   
         */
        
        private static Func<TValue, string> DefaultProcessor<TValue>(MonitorProfile profile)
        {
            var stringBuilder = new StringBuilder();
            var label = profile.FormatData.Label;
            return (value) =>
            {
                stringBuilder.Clear();
                stringBuilder.Append(label);
                stringBuilder.Append(": ");
                stringBuilder.Append(value?.ToString() ?? NULL);
                return stringBuilder.ToString();
            };
        }

        /*
         * Formatted   
         */
        
        private static Func<TValue, string> FormattedProcessor<TValue>(MonitorProfile profile)
        {
            var stringBuilder = new StringBuilder();
            var label = profile.FormatData.Label;
            return (value) =>
            {
                stringBuilder.Clear();
                stringBuilder.Append(label);
                stringBuilder.Append(": ");
                stringBuilder.Append((value as IFormattable)?.ToString(profile.FormatData.Format, null) ?? NULL);
                return stringBuilder.ToString();
            };
        }

        /*
         * Unity Objects   
         */
        
        private static Func<UnityEngine.Object, string> UnityEngineObjectProcessor(MonitorProfile profile)
        {
            var name = profile.FormatData.Label;
            var stringBuilder = new StringBuilder();

            return (value) =>
            {
                stringBuilder.Clear();
                stringBuilder.Append(name);
                stringBuilder.Append(": ");
                stringBuilder.Append((value != null ? value.ToString() : NULL));
                return stringBuilder.ToString();
            };
        }
        
                
        private static Func<Transform, string> TransformProcessor(MonitorProfile profile)
        {
            var sb = new StringBuilder();
            var name = profile.FormatData.Label;
            var nullString = $"{name}: {NULL}";
            var indentValue = CreateIndentValueForProfile(profile) * 2;
            var cachedString = default(string);
            
            return (transform) =>
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
                sb.Append("\n-");
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
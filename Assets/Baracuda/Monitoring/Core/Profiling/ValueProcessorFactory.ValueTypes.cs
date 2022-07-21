// Copyright (c) 2022 Jonathan Lang

using System;
using System.Text;
using Baracuda.Monitoring.Core.Utilities;
using Baracuda.Monitoring.Interface;
using UnityEngine;

namespace Baracuda.Monitoring.Core.Profiling
{
    internal static partial class ValueProcessorFactory
    {
        /*
         * General   
         */
        
        private static Func<TValue, string> ValueTypeProcessor<TValue>(IFormatData formatData)
        {
            var stringBuilder = new StringBuilder();
            var label = formatData.Label;
            return (value) =>
            {
                stringBuilder.Clear();
                stringBuilder.Append(label);
                stringBuilder.Append(": ");
                var str = value.ToString();
                stringBuilder.Append(str);
                return stringBuilder.ToString();
            };
        }
        
        /*
         * Vector3 
         */
        
        private static Func<Vector3, string> Vector3Processor(IFormatData formatData)
        {
            var format = formatData.Format;
            var name = formatData.Label;
            var stringBuilder = new StringBuilder();

            if (format != null)
            {
                return (value) =>
                {
                    stringBuilder.Clear();
                    stringBuilder.Append(name);
                    stringBuilder.Append(": X:");
                    stringBuilder.Append(xColor);
                    stringBuilder.Append('[');
                    stringBuilder.Append(value.x.ToString(format));
                    stringBuilder.Append("]</color> ");
                    stringBuilder.Append("Y:");
                    stringBuilder.Append(yColor);
                    stringBuilder.Append('[');
                    stringBuilder.Append(value.y.ToString(format));
                    stringBuilder.Append("]</color> ");
                    stringBuilder.Append("Z:");
                    stringBuilder.Append(zColor);
                    stringBuilder.Append('[');
                    stringBuilder.Append(value.z.ToString(format));
                    stringBuilder.Append("]</color>");
                    return stringBuilder.ToString();
                };
            }
            else
            {
                return (value) =>
                {
                    stringBuilder.Clear();
                    stringBuilder.Append(name);
                    stringBuilder.Append(": X:");
                    stringBuilder.Append(xColor);
                    stringBuilder.Append('[');
                    stringBuilder.Append(value.x.ToString("0.00"));
                    stringBuilder.Append("]</color> ");
                    stringBuilder.Append("Y:");
                    stringBuilder.Append(yColor);
                    stringBuilder.Append('[');
                    stringBuilder.Append(value.y.ToString("0.00"));
                    stringBuilder.Append("]</color> ");
                    stringBuilder.Append("Z:");
                    stringBuilder.Append(zColor);
                    stringBuilder.Append('[');
                    stringBuilder.Append(value.z.ToString("0.00"));
                    stringBuilder.Append("]</color>");
                    return stringBuilder.ToString();
                };
            }
        }

        /*
         * Vector2   
         */

        private static Func<Vector2, string> Vector2Processor(IFormatData formatData)
        {
            var format = formatData.Format;
            var label = formatData.Label;
            var stringBuilder = new StringBuilder();

            if (format != null)
            {
                return (value) =>
                {
                    stringBuilder.Clear();
                    stringBuilder.Append(label);
                    stringBuilder.Append(": X:");
                    stringBuilder.Append(xColor);
                    stringBuilder.Append('[');
                    stringBuilder.Append(value.x.ToString(format));
                    stringBuilder.Append("]</color> Y:");
                    stringBuilder.Append(yColor);
                    stringBuilder.Append(value.y.ToString(format));
                    stringBuilder.Append("]</color>");

                    return stringBuilder.ToString();
                };
            }
            else
            {
                return (value) =>
                {
                    stringBuilder.Clear();
                    stringBuilder.Append(label);
                    stringBuilder.Append(": X:");
                    stringBuilder.Append(xColor);
                    stringBuilder.Append('[');
                    stringBuilder.Append(value.x.ToString("0.00"));
                    stringBuilder.Append("]</color> Y:");
                    stringBuilder.Append(yColor);
                    stringBuilder.Append(value.y.ToString("0.00"));
                    stringBuilder.Append("]</color>");

                    return stringBuilder.ToString();
                };
            }
        }
        
         /*
         * Quaternion   
         */

        private static Func<Quaternion, string> QuaternionProcessor(IFormatData formatData)
        {
            var format = formatData.Format;
            var name = formatData.Label;
            var stringBuilder = new StringBuilder();


            return format != null
                ? (Func<Quaternion, string>) ((value) =>
                {
                    stringBuilder.Clear();
                    stringBuilder.Append(name);
                    stringBuilder.Append(": X:");
                    stringBuilder.Append(xColor);
                    stringBuilder.Append('[');
                    stringBuilder.Append(value.x.ToString(format));
                    stringBuilder.Append("]</color> ");
                    stringBuilder.Append("Y:");
                    stringBuilder.Append(yColor);
                    stringBuilder.Append('[');
                    stringBuilder.Append(value.y.ToString(format));
                    stringBuilder.Append("]</color> ");
                    stringBuilder.Append("Z:");
                    stringBuilder.Append(zColor);
                    stringBuilder.Append('[');
                    stringBuilder.Append(value.z.ToString(format));
                    stringBuilder.Append("]</color>");
                    stringBuilder.Append("W:");
                    stringBuilder.Append(wColor);
                    stringBuilder.Append('[');
                    stringBuilder.Append(value.w.ToString(format));
                    stringBuilder.Append("]</color>");
                    return stringBuilder.ToString();
                })
                : (value) =>
                {
                    stringBuilder.Clear();
                    stringBuilder.Append(name);
                    stringBuilder.Append(": X:");
                    stringBuilder.Append(xColor);
                    stringBuilder.Append('[');
                    stringBuilder.Append(value.x.ToString("0.00"));
                    stringBuilder.Append("]</color> ");
                    stringBuilder.Append("Y:");
                    stringBuilder.Append(yColor);
                    stringBuilder.Append('[');
                    stringBuilder.Append(value.y.ToString("0.00"));
                    stringBuilder.Append("]</color> ");
                    stringBuilder.Append("Z:");
                    stringBuilder.Append(zColor);
                    stringBuilder.Append('[');
                    stringBuilder.Append(value.z.ToString("0.00"));
                    stringBuilder.Append("]</color>");
                    stringBuilder.Append("W:");
                    stringBuilder.Append(wColor);
                    stringBuilder.Append('[');
                    stringBuilder.Append(value.w.ToString("0.00"));
                    stringBuilder.Append("]</color>");
                    return stringBuilder.ToString();
                };
        }
        
        /*
         * Color   
         */
        
        private static Func<Color, string> ColorProcessor(IFormatData formatData)
        {
            var format = formatData.Format;
            var name = formatData.Label;
            var stringBuilder = new StringBuilder();

            if (format != null)
            {
                return (value) =>
                {
                    stringBuilder.Clear();
                    stringBuilder.Append(name);
                    stringBuilder.Append(": ");
                    stringBuilder.Append(value.ToString(format).Colorize(value));
                    return stringBuilder.ToString();
                };
            }
            else
            {
                return (value) =>
                {
                    stringBuilder.Clear();
                    stringBuilder.Append(name);
                    stringBuilder.Append(": ");
                    stringBuilder.Append(value.ToString().Colorize(value));
                    return stringBuilder.ToString();
                };
            }
        }

        /*
         * Color32   
         */
        
        private static Func<Color32, string> Color32Processor(IFormatData formatData)
        {
            var format = formatData.Format;
            var name = formatData.Label;
            var stringBuilder = new StringBuilder();

            if (format != null)
            {
                return (value) =>
                {
                    stringBuilder.Clear();
                    stringBuilder.Append(name);
                    stringBuilder.Append(": ");
                    stringBuilder.Append(value.ToString(format).Colorize(value));
                    return stringBuilder.ToString();
                };
            }
            else
            {
                return (value) =>
                {
                    stringBuilder.Clear();
                    stringBuilder.Append(name);
                    stringBuilder.Append(": ");
                    stringBuilder.Append(value.ToString().Colorize(value));
                    return stringBuilder.ToString();
                };
            }
        }
    }
}
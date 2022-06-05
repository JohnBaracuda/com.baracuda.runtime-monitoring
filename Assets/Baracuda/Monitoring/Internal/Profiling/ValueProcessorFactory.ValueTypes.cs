// Copyright (c) 2022 Jonathan Lang
using System;
using System.Globalization;
using System.Text;
using Baracuda.Monitoring.Internal.Utilities;
using UnityEngine;

namespace Baracuda.Monitoring.Internal.Profiling
{
    internal static partial class ValueProcessorFactory
    {
        /*
         * General   
         */
        
        private static Func<TValue, string> ValueTypeProcessor<TValue>(MonitorProfile profile)
        {
            var stringBuilder = new StringBuilder();
            var label = profile.FormatData.Label;
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
        
        private static Func<Vector3, string> Vector3Processor(MonitorProfile profile)
        {
            var format = profile.FormatData.Format;
            var name = profile.FormatData.Label;
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

        private static Func<Vector2, string> Vector2Processor(MonitorProfile profile)
        {
            var format = profile.FormatData.Format;
            var label = profile.FormatData.Label;
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

        private static Func<Quaternion, string> QuaternionProcessor(MonitorProfile profile)
        {
            var format = profile.FormatData.Format;
            var name = profile.FormatData.Label;
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
        
        private static Func<Color, string> ColorProcessor(MonitorProfile profile)
        {
            var format = profile.FormatData.Format;
            var name = profile.FormatData.Label;
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
        
        private static Func<Color32, string> Color32Processor(MonitorProfile profile)
        {
            var format = profile.FormatData.Format;
            var name = profile.FormatData.Label;
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
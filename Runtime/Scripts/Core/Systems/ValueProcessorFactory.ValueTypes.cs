// Copyright (c) 2022 Jonathan Lang

using Baracuda.Monitoring.Types;
using System;
using System.Text;
using UnityEngine;

namespace Baracuda.Monitoring.Systems
{
    internal partial class ValueProcessorFactory
    {
        /*
         * General
         */

        private Func<TValue, string> ValueTypeProcessor<TValue>(IFormatData formatData)
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

        private Func<Vector3, string> Vector3Processor(IFormatData formatData)
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
                    stringBuilder.Append(_xColor);
                    stringBuilder.Append('[');
                    stringBuilder.Append(value.x.ToString(format));
                    stringBuilder.Append("]</color> ");
                    stringBuilder.Append("Y:");
                    stringBuilder.Append(_yColor);
                    stringBuilder.Append('[');
                    stringBuilder.Append(value.y.ToString(format));
                    stringBuilder.Append("]</color> ");
                    stringBuilder.Append("Z:");
                    stringBuilder.Append(_zColor);
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
                    stringBuilder.Append(_xColor);
                    stringBuilder.Append('[');
                    stringBuilder.Append(value.x.ToString("0.00"));
                    stringBuilder.Append("]</color> ");
                    stringBuilder.Append("Y:");
                    stringBuilder.Append(_yColor);
                    stringBuilder.Append('[');
                    stringBuilder.Append(value.y.ToString("0.00"));
                    stringBuilder.Append("]</color> ");
                    stringBuilder.Append("Z:");
                    stringBuilder.Append(_zColor);
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

        private Func<Vector2, string> Vector2Processor(IFormatData formatData)
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
                    stringBuilder.Append(_xColor);
                    stringBuilder.Append('[');
                    stringBuilder.Append(value.x.ToString(format));
                    stringBuilder.Append("]</color> Y:");
                    stringBuilder.Append(_yColor);
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
                    stringBuilder.Append(_xColor);
                    stringBuilder.Append('[');
                    stringBuilder.Append(value.x.ToString("0.00"));
                    stringBuilder.Append("]</color> Y:");
                    stringBuilder.Append(_yColor);
                    stringBuilder.Append(value.y.ToString("0.00"));
                    stringBuilder.Append("]</color>");

                    return stringBuilder.ToString();
                };
            }
        }

         /*
         * Quaternion
         */

        private Func<Quaternion, string> QuaternionProcessor(IFormatData formatData)
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
                    stringBuilder.Append(_xColor);
                    stringBuilder.Append('[');
                    stringBuilder.Append(value.x.ToString(format));
                    stringBuilder.Append("]</color> ");
                    stringBuilder.Append("Y:");
                    stringBuilder.Append(_yColor);
                    stringBuilder.Append('[');
                    stringBuilder.Append(value.y.ToString(format));
                    stringBuilder.Append("]</color> ");
                    stringBuilder.Append("Z:");
                    stringBuilder.Append(_zColor);
                    stringBuilder.Append('[');
                    stringBuilder.Append(value.z.ToString(format));
                    stringBuilder.Append("]</color>");
                    stringBuilder.Append("W:");
                    stringBuilder.Append(_wColor);
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
                    stringBuilder.Append(_xColor);
                    stringBuilder.Append('[');
                    stringBuilder.Append(value.x.ToString("0.00"));
                    stringBuilder.Append("]</color> ");
                    stringBuilder.Append("Y:");
                    stringBuilder.Append(_yColor);
                    stringBuilder.Append('[');
                    stringBuilder.Append(value.y.ToString("0.00"));
                    stringBuilder.Append("]</color> ");
                    stringBuilder.Append("Z:");
                    stringBuilder.Append(_zColor);
                    stringBuilder.Append('[');
                    stringBuilder.Append(value.z.ToString("0.00"));
                    stringBuilder.Append("]</color>");
                    stringBuilder.Append("W:");
                    stringBuilder.Append(_wColor);
                    stringBuilder.Append('[');
                    stringBuilder.Append(value.w.ToString("0.00"));
                    stringBuilder.Append("]</color>");
                    return stringBuilder.ToString();
                };
        }

        /*
         * Color
         */

        private Func<Color, string> ColorProcessor(IFormatData formatData)
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
                    stringBuilder.Append(value.ToString(format).ColorizeString(value));
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
                    stringBuilder.Append(value.ToString().ColorizeString(value));
                    return stringBuilder.ToString();
                };
            }
        }

        /*
         * Color32
         */

        private Func<Color32, string> Color32Processor(IFormatData formatData)
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
                    stringBuilder.Append(value.ToString(format).ColorizeString(value));
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
                    stringBuilder.Append(value.ToString().ColorizeString(value));
                    return stringBuilder.ToString();
                };
            }
        }
    }
}
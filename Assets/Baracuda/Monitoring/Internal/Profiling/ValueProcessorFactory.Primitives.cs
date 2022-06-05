// Copyright (c) 2022 Jonathan Lang
using System;
using System.Text;

namespace Baracuda.Monitoring.Internal.Profiling
{
    internal static partial class ValueProcessorFactory
    {
        /*
         * Integers   
         */
        
        private static Func<int, string> Int32Processor(MonitorProfile profile)
        {
            var stringBuilder = new StringBuilder();
            var label = profile.FormatData.Label;
            var format = profile.FormatData.Format;
            if (format != null)
            {
                return (value) =>
                {
                    stringBuilder.Clear();
                    stringBuilder.Append(label);
                    stringBuilder.Append(": ");
                    stringBuilder.Append(value.ToString(format));
                    return stringBuilder.ToString();
                };
            }
            else
            {
                return (value) =>
                {
                    stringBuilder.Clear();
                    stringBuilder.Append(label);
                    stringBuilder.Append(": ");
                    stringBuilder.Append(value);
                    return stringBuilder.ToString();
                };
            }
        }

        private static Func<long, string> Int64Processor(MonitorProfile profile)
        {
            var stringBuilder = new StringBuilder();
            var label = profile.FormatData.Label;
            return (value) =>
            {
                stringBuilder.Clear();
                stringBuilder.Append(label);
                stringBuilder.Append(": ");
                stringBuilder.Append(value);
                return stringBuilder.ToString();
            };
        }

        /*
         * Floating Points   
         */

        private static Func<float, string> SingleProcessor(MonitorProfile profile)
        {
            var stringBuilder = new StringBuilder();
            var label = profile.FormatData.Label;
            return (value) =>
            {
                stringBuilder.Clear();
                stringBuilder.Append(label);
                stringBuilder.Append(": ");
                stringBuilder.Append(value.ToString("0.00"));
                return stringBuilder.ToString();
            };
        }

        private static Func<double, string> DoubleProcessor(MonitorProfile profile)
        {
            var stringBuilder = new StringBuilder();
            var label = profile.FormatData.Label;
            return (value) =>
            {
                stringBuilder.Clear();
                stringBuilder.Append(label);
                stringBuilder.Append(": ");
                stringBuilder.Append(value.ToString("0.00"));
                return stringBuilder.ToString();
            };
        }

        /*
         * Boolean   
         */
                
        private static Func<bool, string> CreateBooleanProcessor(MonitorProfile profile)
        {
            var trueString  =  $"{profile.FormatData.Label}: {trueColored}";
            var falseString =  $"{profile.FormatData.Label}: {falseColored}";
            return (value) => value ? trueString : falseString;
        }
    }
}
// Copyright (c) 2022 Jonathan Lang

using System;
using System.Text;

namespace Baracuda.Monitoring.Systems
{
    internal partial class ValueProcessorFactory
    {
        /*
         * Integers   
         */
        
        private Func<int, string> Int32Processor(IFormatData formatData)
        {
            var stringBuilder = new StringBuilder();
            var label = formatData.Label;
            var format = formatData.Format;
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

        private Func<long, string> Int64Processor(IFormatData formatData)
        {
            var stringBuilder = new StringBuilder();
            var label = formatData.Label;
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

        private Func<float, string> SingleProcessor(IFormatData formatData)
        {
            var stringBuilder = new StringBuilder();
            var label = formatData.Label;
            return (value) =>
            {
                stringBuilder.Clear();
                stringBuilder.Append(label);
                stringBuilder.Append(": ");
                stringBuilder.Append(value.ToString("0.00"));
                return stringBuilder.ToString();
            };
        }

        private Func<double, string> DoubleProcessor(IFormatData formatData)
        {
            var stringBuilder = new StringBuilder();
            var label = formatData.Label;
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
                
        private Func<bool, string> CreateBooleanProcessor(IFormatData formatData)
        {
            var trueString  =  $"{formatData.Label}: {_trueColored}";
            var falseString =  $"{formatData.Label}: {_falseColored}";
            return (value) => value ? trueString : falseString;
        }
    }
}
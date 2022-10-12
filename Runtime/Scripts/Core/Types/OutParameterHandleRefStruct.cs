// Copyright (c) 2022 Jonathan Lang

using Baracuda.Monitoring.Utilities.Extensions;
using System;
using System.Text;

namespace Baracuda.Monitoring.Types
{
    /// <summary>
    /// Out parameter handle capable of handing readonly ref struct out parameters in IL2CPP Runtime.
    /// </summary>
    internal class OutParameterHandleRefStruct : OutParameterHandle
    {
        private readonly IFormatData _formatData;
        private readonly StringBuilder _stringBuilder = new StringBuilder();
        private readonly Func<object, string> _valueProcessor;

        public override string GetValueAsString(object value)
        {
            return _valueProcessor(value);
        }

        public OutParameterHandleRefStruct(Type type, IFormatData formatData)
        {
            _formatData = formatData;
            _valueProcessor = CreateValueProcessor(type, formatData);
        }

        private Func<object, string> CreateValueProcessor(Type type, IFormatData formatData)
        {

            if (type.HasInterface<IFormattable>())
            {
                return (value) =>
                {
                    _stringBuilder.Clear();
                    _stringBuilder.Append(_formatData.Label);
                    _stringBuilder.Append(' ');
                    _stringBuilder.Append(((IFormattable) value).ToString(formatData.Format, null));
                    return _stringBuilder.ToString();
                };
            }
            else
            {
                return (value) =>
                {
                    _stringBuilder.Clear();
                    _stringBuilder.Append(_formatData.Label);
                    _stringBuilder.Append(' ');
                    _stringBuilder.Append(value);
                    return _stringBuilder.ToString();
                };
            }
        }
    }
}

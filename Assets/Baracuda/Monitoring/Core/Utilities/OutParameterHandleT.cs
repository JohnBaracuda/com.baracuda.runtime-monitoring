// Copyright (c) 2022 Jonathan Lang

using System;
using Baracuda.Monitoring.Core.Profiling;
using Baracuda.Monitoring.Interface;

namespace Baracuda.Monitoring.Core.Utilities
{
    public class OutParameterHandleT<TValue> : OutParameterHandle
    {
        public override string GetValueAsString(object value)
        {
            return _processor((TValue)value);
        }

        private readonly Func<TValue, string> _processor;
        
        private OutParameterHandleT(IFormatData formatData)
        {
            _processor = ValueProcessorFactory.CreateProcessorForType<TValue>(formatData);
        }

        public override string ToString()
        {
            return _processor(default);
        }
    }
}
// Copyright (c) 2022 Jonathan Lang

using System;
using Baracuda.Monitoring.API;
using Baracuda.Monitoring.Source.Interfaces;

namespace Baracuda.Monitoring.Source.Types
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
            _processor = MonitoringSystems.Resolve<IValueProcessorFactory>().CreateProcessorForType<TValue>(formatData);
        }

        public override string ToString()
        {
            return _processor(default);
        }
    }
}
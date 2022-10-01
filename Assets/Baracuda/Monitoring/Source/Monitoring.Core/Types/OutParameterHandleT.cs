// Copyright (c) 2022 Jonathan Lang

using Baracuda.Monitoring.Core.Interfaces;
using System;

namespace Baracuda.Monitoring.Core.Types
{
    internal class OutParameterHandleT<TValue> : OutParameterHandle
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
// Copyright (c) 2022 Jonathan Lang

namespace Baracuda.Monitoring.Types
{
    internal readonly struct MethodResult<TValue>
    {
        public readonly TValue Value;
        private readonly string _toStringValue;

        public MethodResult(TValue value, string toStringValue)
        {
            Value = value;
            _toStringValue = toStringValue;
        }

        public override string ToString()
        {
            return _toStringValue;
        }
    }
}
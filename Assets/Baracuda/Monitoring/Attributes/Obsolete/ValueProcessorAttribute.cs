// Copyright (c) 2022 Jonathan Lang

using System;

namespace Baracuda.Monitoring.Obsolete
{
    [Obsolete("use MValueProcessorAttribute instead!")]
    public sealed class ValueProcessorAttribute : MValueProcessorAttribute
    {
        public ValueProcessorAttribute(string processorMethod) : base(processorMethod)
        {
        }
    }
}
// Copyright (c) 2022 Jonathan Lang (CC BY-NC-SA 4.0)
using System;

namespace Baracuda.Monitoring.Internal.Exceptions
{
    /// <summary>
    /// Exception occurs if a custom value processor cannot be found, either because its was misspelled or because it is not static.
    /// </summary>
    internal class ProcessorNotFoundException : Exception
    {
        public ProcessorNotFoundException(string processor, Type declaringType) 
            : base($"Processor: {processor} in {declaringType.Name} was not found! Only static methods are valid value processors")
        {
        }
    }
}
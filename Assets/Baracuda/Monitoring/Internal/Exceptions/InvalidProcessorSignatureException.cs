// Copyright (c) 2022 Jonathan Lang (CC BY-NC-SA 4.0)
using System;

namespace Baracuda.Monitoring.Internal.Exceptions
{
    /// <summary>
    /// Exception occurs if a custom value processors signature is not valid.
    /// </summary>
    internal class InvalidProcessorSignatureException : Exception
    {
        public InvalidProcessorSignatureException(string processor, Type declaringType) 
            : base($"Processor: {processor} in {declaringType.Name} does not have a valid value processor signature!")
        {
        }
    }
}
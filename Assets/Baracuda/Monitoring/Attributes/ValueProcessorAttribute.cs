// Copyright (c) 2022 Jonathan Lang (CC BY-NC-SA 4.0)
using System;
using UnityEngine.Scripting;

namespace Baracuda.Monitoring
{
    /// <summary>
    /// Set a method as a custom value processor for a monitored member.
    /// The method must return a string and accept the value of the monitored member.
    /// </summary>
    [Preserve]
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public sealed class ValueProcessorAttribute : Attribute
    {
        public string Processor { get; }
        
        /// <summary>
        /// Set a method as a custom value processor for a monitored member.
        /// The method must return a string and accept the value of the monitored member.
        /// </summary>
        /// <param name="processorMethod">The name of the method you want to use as a value processor.</param>
        /// <footer>Note: use the nameof keyword to pass the name of the method.</footer> 
        public ValueProcessorAttribute(string processorMethod)
        {
            Processor = processorMethod;
        }
    }
}
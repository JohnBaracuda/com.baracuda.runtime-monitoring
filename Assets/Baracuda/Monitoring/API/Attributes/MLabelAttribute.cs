// Copyright (c) 2022 Jonathan Lang

using System;
using UnityEngine.Scripting;

namespace Baracuda.Monitoring
{
    [Preserve]
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Event | AttributeTargets.Method | AttributeTargets.Class)]
    public class MLabelAttribute : MonitoringMetaAttribute
    {
        /// <summary>
        /// Custom label for the member. 
        /// </summary>
        public readonly string Label;

        /// <summary>
        /// Custom label for the member. 
        /// </summary>
        public MLabelAttribute(string label)
        {
            Label = label;
        }
    }
}
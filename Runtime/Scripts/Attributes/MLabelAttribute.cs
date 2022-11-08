// Copyright (c) 2022 Jonathan Lang

using System;
using UnityEngine.Scripting;

namespace Baracuda.Monitoring
{
    /// <summary>
    /// Custom label for the member.
    /// </summary>
    [Preserve]
    [AttributeUsage(Targets)]
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
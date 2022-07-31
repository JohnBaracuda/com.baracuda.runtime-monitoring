// Copyright (c) 2022 Jonathan Lang

using System;
using UnityEngine.Scripting;

namespace Baracuda.Monitoring
{
    [Preserve]
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Event | AttributeTargets.Method | AttributeTargets.Class)]
    public class MFormatAttribute : MonitoringMetaAttribute
    {
        /// <summary>
        /// Custom format string used to display the members value if possible.
        /// </summary>
        public readonly string Format;

        /// <summary>
        /// Custom format string used to display the members value if possible.
        /// </summary>
        public MFormatAttribute(string format)
        {
            Format = format;
        }
    }
}
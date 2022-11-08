// Copyright (c) 2022 Jonathan Lang

using System;
using UnityEngine.Scripting;

namespace Baracuda.Monitoring
{
    /// <summary>
    /// The elementIndent of individual elements of a displayed collection.
    /// This property will only have an effect on collections.
    /// </summary>
    [Preserve]
    [AttributeUsage(Targets)]
    public class MElementIndentAttribute : MonitoringMetaAttribute
    {
        /// <summary>
        /// The elementIndent of individual elements of a displayed collection.
        /// This property will only have an effect on collections.
        /// </summary>
        public readonly int ElementIndent;

        /// <summary>
        /// The elementIndent of individual elements of a displayed collection.
        /// This property will only have an effect on collections.
        /// </summary>
        public MElementIndentAttribute(int elementIndent)
        {
            ElementIndent = elementIndent;
        }
    }
}
// Copyright (c) 2022 Jonathan Lang

using System;
using UnityEngine.Scripting;

namespace Baracuda.Monitoring
{
    /// <summary>
    /// The preferred position of an individual UIElement on the canvas.
    /// </summary>
    [Preserve]
    [AttributeUsage(Targets)]
    public class MPositionAttribute : MonitoringMetaAttribute
    {
        /// <summary>
        /// The preferred position of an individual UIElement on the canvas.
        /// </summary>
        public readonly UIPosition Position;

        /// <summary>
        /// The preferred position of an individual UIElement on the canvas.
        /// </summary>
        public MPositionAttribute(UIPosition position)
        {
            Position = position;
        }
    }
}
// Copyright (c) 2022 Jonathan Lang

using System;
using UnityEngine.Scripting;

namespace Baracuda.Monitoring
{
    [Preserve]
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Event | AttributeTargets.Method | AttributeTargets.Class)]
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
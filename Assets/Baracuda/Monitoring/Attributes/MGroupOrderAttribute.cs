// Copyright (c) 2022 Jonathan Lang

using System;
using UnityEngine.Scripting;

namespace Baracuda.Monitoring
{
    [Preserve]
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Event | AttributeTargets.Method | AttributeTargets.Class)]
    public class MGroupOrderAttribute : MonitoringMetaAttribute
    {
        public readonly int Order;

        /// <summary>
        /// Set a elements order for its UI group.
        /// </summary>
        public MGroupOrderAttribute(int order)
        {
            Order = order;
        }
    }
}
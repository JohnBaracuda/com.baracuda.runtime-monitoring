// Copyright (c) 2022 Jonathan Lang

using System;
using UnityEngine.Scripting;

namespace Baracuda.Monitoring
{
    /// <summary>
    /// Set a elements order in its UI group.
    /// </summary>
    [Preserve]
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Event | AttributeTargets.Method | AttributeTargets.Class)]
    public class MOrderAttribute : MonitoringMetaAttribute
    {
        /// <summary>
        /// Signed order value.
        /// </summary>
        public readonly int Order;

        /// <summary>
        /// Set a elements order in its UI group.
        /// </summary>
        public MOrderAttribute(int order)
        {
            Order = order;
        }
    }
}
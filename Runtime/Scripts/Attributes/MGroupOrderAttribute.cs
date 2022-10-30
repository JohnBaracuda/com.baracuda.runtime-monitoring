// Copyright (c) 2022 Jonathan Lang

using System;
using UnityEngine.Scripting;

namespace Baracuda.Monitoring
{
    /// <summary>
    /// Set a elements order for its UI group.
    /// </summary>
    [Preserve]
    [AttributeUsage(Targets)]
    public class MGroupOrderAttribute : MonitoringMetaAttribute
    {
        /// <summary>
        /// Signed order value.
        /// </summary>
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
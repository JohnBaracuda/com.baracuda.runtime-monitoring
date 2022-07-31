// Copyright (c) 2022 Jonathan Lang

using System;
using UnityEngine.Scripting;

namespace Baracuda.Monitoring
{
    [Preserve]
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Event | AttributeTargets.Method | AttributeTargets.Class)]
    public class MTextAlignAttribute : MonitoringMetaAttribute
    {
        /// <summary>
        /// Horizontal Text Align
        /// </summary>
        public readonly HorizontalTextAlign TextAlign;

        /// <summary>
        /// Horizontal Text Align
        /// </summary>
        public MTextAlignAttribute(HorizontalTextAlign textAlign)
        {
            TextAlign = textAlign;
        }
    }
}
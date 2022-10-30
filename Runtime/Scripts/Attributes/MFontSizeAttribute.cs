// Copyright (c) 2022 Jonathan Lang

using System;
using UnityEngine.Scripting;

namespace Baracuda.Monitoring
{
    /// <summary>
    /// Set the fontsize for the UI.
    /// </summary>
    [Preserve]
    [AttributeUsage(Targets)]
    public class MFontSizeAttribute : MonitoringMetaAttribute
    {
        /// <summary>
        /// Set the fontsize for the UI.
        /// </summary>
        public readonly int FontSize;

        /// <summary>
        /// Set the fontsize for the UI.
        /// </summary>
        public MFontSizeAttribute(int fontSize)
        {
            FontSize = fontSize;
        }
    }
}
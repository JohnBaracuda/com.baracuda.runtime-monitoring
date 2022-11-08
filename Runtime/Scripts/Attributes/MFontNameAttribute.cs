// Copyright (c) 2022 Jonathan Lang

using System;
using UnityEngine.Scripting;

namespace Baracuda.Monitoring
{
    /// <summary>
    /// Pass the name of a custom font style that will be used fot the target member.
    /// FontName assets must be registered to the UI Controller.
    /// </summary>
    [Preserve]
    [AttributeUsage(Targets)]
    public class MFontNameAttribute : MonitoringMetaAttribute
    {
        /// <summary>
        /// Name of the font.
        /// </summary>
        public readonly string FontName;

        /// <summary>
        /// Hash of the name of the font.
        /// </summary>
        public readonly int FontHash;

        /// <summary>
        /// Pass the name of a custom font style that will be used fot the target member.
        /// FontName assets must be registered to the UI Controller.
        /// </summary>
        public MFontNameAttribute(string fontName)
        {
            FontName = fontName;
            FontHash = fontName.GetHashCode();
        }
    }
}
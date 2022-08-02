// Copyright (c) 2022 Jonathan Lang

using System;
using UnityEngine.Scripting;

namespace Baracuda.Monitoring
{
    [Preserve]
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Event | AttributeTargets.Method | AttributeTargets.Class)]
    public class MFontNameAttribute : MonitoringMetaAttribute
    {
        public readonly string FontName;
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
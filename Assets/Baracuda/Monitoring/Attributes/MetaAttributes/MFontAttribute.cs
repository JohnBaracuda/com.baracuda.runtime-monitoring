// Copyright (c) 2022 Jonathan Lang

using System;
using UnityEngine.Scripting;

namespace Baracuda.Monitoring
{
    [Preserve]
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Event | AttributeTargets.Method)]
    public class MFontAttribute : MonitoringMetaAttribute
    {
        public readonly int FontHash;

        /// <summary>
        /// Pass the name of a custom font style that will be used fot the target member.
        /// Font assets must be registered to the UI Controller.
        /// </summary>
        public MFontAttribute(string fontName)
        {
            FontHash = fontName.GetHashCode();
        }
    }
}
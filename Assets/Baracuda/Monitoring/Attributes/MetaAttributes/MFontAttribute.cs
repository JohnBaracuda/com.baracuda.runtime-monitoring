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

        public MFontAttribute(string fontName)
        {
            FontHash = fontName.GetHashCode();
        }
    }
}
using System;
using UnityEngine.Scripting;

namespace Baracuda.Monitoring
{
    [Preserve]
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Event | AttributeTargets.Method)]
    public class MFontAttribute : MonitoringMetaAttribute
    {
        public readonly string FontName;

        public MFontAttribute(string fontName)
        {
            FontName = fontName;
        }
    }
}
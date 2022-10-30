// Copyright (c) 2022 Jonathan Lang

using System;
using UnityEngine;
using UnityEngine.Scripting;

namespace Baracuda.Monitoring
{
    /// <summary>
    /// Base type for attributes to set custom color values.
    /// </summary>
    [Preserve]
    [AttributeUsage(Targets, AllowMultiple = true)]
    public abstract class MColorAttribute : MonitoringMetaAttribute
    {
        /// <summary>
        /// Parsed color based on passed primitive values.
        /// </summary>
        public readonly Color ColorValue;

        /// <summary>
        /// Base type for attributes to set custom color values.
        /// </summary>
        protected MColorAttribute(float r, float g, float b, float a = 1)
        {
            ColorValue = new Color(r, g, b, a);
        }

        /// <summary>
        /// Base type for attributes to set custom color values.
        /// </summary>
        protected MColorAttribute(ColorPreset colorPreset)
        {
            ColorValue = colorPreset.ToColor();
        }

        /// <summary>
        /// Base type for attributes to set custom color values.
        /// </summary>
        protected MColorAttribute(string colorValueHex)
        {
            if (!ColorUtility.TryParseHtmlString(colorValueHex, out ColorValue))
            {
                Debug.LogError($"[{GetType().Name}] {colorValueHex} is not a valid color hexadecimal value!");
            }
        }
    }
}
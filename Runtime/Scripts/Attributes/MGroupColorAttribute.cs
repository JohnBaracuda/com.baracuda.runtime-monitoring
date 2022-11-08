// Copyright (c) 2022 Jonathan Lang

using System;
using UnityEngine.Scripting;

namespace Baracuda.Monitoring
{
    /// <summary>
    /// Determine the background color for the group of the displayed value.
    /// </summary>
    [Preserve]
    [AttributeUsage(Targets)]
    public class MGroupColorAttribute : MColorAttribute
    {
        /// <summary>
        /// Determine the background color for the group of the displayed value.
        /// </summary>
        /// <param name="r">Red channel value</param>
        /// <param name="g">Green channel value</param>
        /// <param name="b">Blue channel value</param>
        /// <param name="a">Alpha channel value</param>
        public MGroupColorAttribute(float r, float g, float b, float a = 1) : base(r, g, b, a)
        {
        }

        /// <summary>
        /// Determine the background color for the group of the displayed value.
        /// </summary>
        /// <param name="colorPreset">Chose a preset of predefined color values</param>
        public MGroupColorAttribute(ColorPreset colorPreset)  : base(colorPreset)
        {
        }

        /// <summary>
        /// Determine the background color for the group of the displayed value.
        /// </summary>
        /// <param name="colorValueHex">Set the color via hexadecimal value</param>
        public MGroupColorAttribute(string colorValueHex)  : base(colorValueHex)
        {
        }
    }
}
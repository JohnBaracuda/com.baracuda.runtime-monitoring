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
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Event | AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = true)]
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

    //------------------------------------------------------------------------------------------------------------------

    /// <summary>
    /// Enum contains some easily accessible color presets.
    /// </summary>
    public enum ColorPreset
    {
        /// <summary> 100% Transparent </summary>
        Transparent = 0,
        /// <summary> 100% Black </summary>
        Black = 1,
        /// <summary> 100% White </summary>
        White = 2,
        /// <summary> 50% Gray </summary>
        Gray = 3,
        /// <summary> 50% Transparent Black </summary>
        TransparentBlack = 4,
        /// <summary> Red Color </summary>
        Red = 5,
        /// <summary> Green Color </summary>
        Green = 6,
        /// <summary> Blue Color </summary>
        Blue = 7,
        /// <summary> Yellow Color </summary>
        Yellow = 8,
        /// <summary> Cyan Color </summary>
        Cyan = 9,
        /// <summary> Magenta Color </summary>
        Magenta = 10,
        /// <summary> Orange Color </summary>
        Orange = 11,
        /// <summary> Lime Color </summary>
        Lime = 12,
        /// <summary> LightBlue Color </summary>
        LightBlue = 13,
    }

    internal static class ColorPresetExtensions
    {
        public static Color ToColor(this ColorPreset colorPreset)
        {
            switch (colorPreset)
            {
                case ColorPreset.Transparent:
                    return Color.clear;
                case ColorPreset.Black:
                    return Color.black;
                case ColorPreset.White:
                    return Color.white;
                case ColorPreset.Gray:
                    return Color.gray;
                case ColorPreset.TransparentBlack:
                    return new Color(0, 0, 0, .5f);

                case ColorPreset.Red:
                    return Color.red;
                case ColorPreset.Green:
                    return Color.green;
                case ColorPreset.Blue:
                    return Color.blue;
                case ColorPreset.Yellow:
                    return Color.yellow;
                case ColorPreset.Cyan:
                    return Color.cyan;
                case ColorPreset.Magenta:
                    return Color.magenta;

                case ColorPreset.Orange:
                    return new Color(1f, 0.5f, 0f);
                case ColorPreset.Lime:
                    return new Color(0.5f, 1f, 0f);
                case ColorPreset.LightBlue:
                    return new Color(0.7f, 0.9f, 1f);
                default:
                    throw new ArgumentOutOfRangeException(nameof(colorPreset), colorPreset, null);
            }
        }
    }
}
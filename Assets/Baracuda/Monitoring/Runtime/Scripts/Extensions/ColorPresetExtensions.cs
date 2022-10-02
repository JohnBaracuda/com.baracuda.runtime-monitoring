using System;
using UnityEngine;

namespace Baracuda.Monitoring
{
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
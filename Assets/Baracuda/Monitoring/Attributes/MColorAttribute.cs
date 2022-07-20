// Copyright (c) 2022 Jonathan Lang

using System;
using UnityEngine;
using UnityEngine.Scripting;

namespace Baracuda.Monitoring
{
    public enum ColorPreset
    {
        Transparent,
        TransparentBlack,
        Black,
        White,
        Gray,
        Red,
    }
    
    [Preserve]
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Event | AttributeTargets.Method, AllowMultiple = true)]
    public abstract class MColorAttribute : MonitoringMetaAttribute
    {
        public readonly Color ColorValue;

        protected MColorAttribute(float r, float g, float b, float a = 1)
        {
            ColorValue = new Color(r, g, b, a);
        }

        protected MColorAttribute(ColorPreset colorPreset)
        {
            switch (colorPreset)
            {
                case ColorPreset.Transparent:
                    ColorValue = Color.clear;
                    break;
                case ColorPreset.Black:
                    ColorValue = Color.black;
                    break;
                case ColorPreset.White:
                    ColorValue = Color.white;
                    break;
                case ColorPreset.Gray:
                    ColorValue = Color.gray;
                    break;
                case ColorPreset.TransparentBlack:
                    ColorValue = new Color(0, 0, 0, .5f);
                    break;
                case ColorPreset.Red:
                    ColorValue = Color.red;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(colorPreset), colorPreset, null);
            }
        }
    }

    [Preserve]
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Event | AttributeTargets.Method)]
    public class MBackgroundColorAttribute : MColorAttribute
    {
        public MBackgroundColorAttribute(float r, float g, float b, float a = 1) : base(r, g, b, a)
        {
        }
        
        public MBackgroundColorAttribute(ColorPreset colorPreset)  : base(colorPreset)
        {
        }
    }
    
    [Preserve]
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Event | AttributeTargets.Method)]
    public class MTextColorAttribute : MColorAttribute
    {
        public MTextColorAttribute(float r, float g, float b, float a = 1) : base(r, g, b, a)
        {
        }

        public MTextColorAttribute(ColorPreset colorPreset)  : base(colorPreset)
        {
        }
    }
    
    [Preserve]
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Event | AttributeTargets.Method)]
    public class MGroupColorAttribute : MColorAttribute
    {
        public MGroupColorAttribute(float r, float g, float b, float a = 1) : base(r, g, b, a)
        {
        }
        
        public MGroupColorAttribute(ColorPreset colorPreset)  : base(colorPreset)
        {
        }
    }
}
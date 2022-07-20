// Copyright (c) 2022 Jonathan Lang

using System;
using UnityEngine;
using UnityEngine.Scripting;

namespace Baracuda.Monitoring
{
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
            ColorValue = colorPreset.ToColor();
        }
    }
}
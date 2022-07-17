// Copyright (c) 2022 Jonathan Lang

using System;
using UnityEngine;
using UnityEngine.Scripting;

namespace Baracuda.Monitoring
{
    [Preserve]
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Event | AttributeTargets.Method)]
    public class MColor : MonitoringMetaAttribute
    {
        public readonly Color Color;
        
        public MColor(float r, float g, float b, float a = 1)
        {
            Color = new Color(r, g, b, a);
        }
    }
}
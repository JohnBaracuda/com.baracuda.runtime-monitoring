// Copyright (c) 2022 Jonathan Lang

using System;
using UnityEngine.Scripting;

namespace Baracuda.Monitoring.Obsolete
{
    [Preserve]
    [Obsolete("use MOptionsAttribute instead!")]
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Event | AttributeTargets.Method | AttributeTargets.Class)]
    public sealed class MFormatOptionsAttribute : MOptionsAttribute
    {
        public MFormatOptionsAttribute(string format) : base(format)
        {
        }
        
        public MFormatOptionsAttribute(UIPosition position) : base(position)
        {
        }
    }
}
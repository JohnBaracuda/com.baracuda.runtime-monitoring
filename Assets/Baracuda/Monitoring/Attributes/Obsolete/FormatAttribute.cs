// Copyright (c) 2022 Jonathan Lang
using System;
using UnityEngine.Scripting;

namespace Baracuda.Monitoring
{
    [Obsolete("use MFormatOptionsAttribute instead!")]
    [Preserve]
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Event)]
    public sealed class FormatAttribute : MFormatOptionsAttribute
    {
        public FormatAttribute(string format) : base(format)
        {
        }
        
        public FormatAttribute(UIPosition position) : base(position)
        {
        }
    }
}
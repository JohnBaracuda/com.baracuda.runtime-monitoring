using System;
using UnityEngine.Scripting;

namespace Baracuda.Monitoring.Obsolete
{
    [Preserve]
    [Obsolete("use MOptionsAttribute instead!")]
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Event | AttributeTargets.Method)]
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
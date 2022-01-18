using System;

namespace Baracuda.Attributes.Monitoring
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Event)]
    public sealed class StyleAttribute : Attribute
    {
        public readonly string[] ClassList;
        
        public StyleAttribute(params string[] classList)
        {
            ClassList = classList;
        }
        
        public StyleAttribute(string @class)
        {
            ClassList = new []{@class};
        }
    }
}
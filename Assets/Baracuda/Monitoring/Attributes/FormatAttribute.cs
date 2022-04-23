using System;
using UnityEngine.Scripting;

namespace Baracuda.Monitoring.Attributes
{
    public enum UIPosition
    {
        TopLeft,
        TopRight,
        BottomLeft,
        BottomRight
    }
    
    [Preserve]
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public sealed class FormatAttribute : MonitoringMetaAttribute
    {
        /// <summary>
        /// Custom format string used to display the members value if possible.
        /// </summary>
        public string Format { get; set; }
        
        /// <summary>
        /// Custom label for the member. 
        /// </summary>
        public string Label { get; set; } = null;
        
        /// <summary>
        /// If the member is a collection, determine if the index of individual elements should be displayed or not.
        /// </summary>
        public bool ShowIndexer { get; set; } = true;
        
        /// <summary>
        /// Set the fontsize for the UI.
        /// </summary>
        public int FontSize { get; set; } = -1;

        /// <summary>
        /// Whether or not the unit should be wrapped in an object or type group.
        /// </summary>
        public bool GroupElement { get; set; } = true;
        
        /// <summary>
        /// The indent of individual elements of a displayed collection.
        /// This property will only have an effect on collections.
        /// </summary>
        public int ElementIndent { get; set; } = -1;
        
        /// <summary>
        /// The preferred position of an individual UIElement on the canvas. 
        /// </summary>
        public UIPosition Position { get; set; } = UIPosition.TopLeft;
        
        
        public FormatAttribute(string format)
        {
            Format = format;
        }

        public FormatAttribute()
        {
        }
    }

    public static class UIPositionExtension
    {
        public static string AsString(this UIPosition target)
        {
            switch (target)
            {
                case UIPosition.TopLeft:
                    return nameof(UIPosition.TopLeft);
                case UIPosition.TopRight:
                    return nameof(UIPosition.TopRight);
                case UIPosition.BottomLeft:
                    return nameof(UIPosition.BottomLeft);
                case UIPosition.BottomRight:
                    return nameof(UIPosition.BottomRight);
                default:
                    throw new ArgumentOutOfRangeException(nameof(target), target, null);
            }
        }
    }
}
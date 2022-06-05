// Copyright (c) 2022 Jonathan Lang
using System;
using UnityEngine.Scripting;

namespace Baracuda.Monitoring
{
    [Preserve]
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Event)]
    public class MFormatOptionsAttribute : MonitoringMetaAttribute
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
        public UIPosition Position { get; set; } = UIPosition.UpperLeft;
        
        
        public MFormatOptionsAttribute(string format)
        {
            Format = format;
        }
        
        public MFormatOptionsAttribute(UIPosition position)
        {
            Position = position;
        }

        public MFormatOptionsAttribute()
        {
        }
    }
    
    /*
     * UI Position
     */

    public enum UIPosition
    {
        UpperLeft = 0,
        UpperRight = 1,
        LowerLeft = 2,
        LowerRight = 3,
        
        [Obsolete("Use UIPosition.UpperLeft instead!")]
        TopLeft = 0,
        [Obsolete("Use UIPosition.UpperRight instead!")]
        TopRight = 1,
        [Obsolete("Use UIPosition.LowerLeft instead!")]
        BottomLeft = 2,
        [Obsolete("Use UIPosition.LowerRight instead!")]
        BottomRight = 3,
    }

    public static class UIPositionExtension
    {
        public static string AsString(this UIPosition target)
        {
            switch ((int)target)
            {
                case (int)UIPosition.UpperLeft:
                    return nameof(UIPosition.UpperLeft);
                case (int)UIPosition.UpperRight:
                    return nameof(UIPosition.UpperRight);
                case (int)UIPosition.LowerLeft:
                    return nameof(UIPosition.LowerLeft);
                case (int)UIPosition.LowerRight:
                    return nameof(UIPosition.LowerRight);
                default:
                    throw new ArgumentOutOfRangeException(nameof(target), target, null);
            }
        }
    }
}
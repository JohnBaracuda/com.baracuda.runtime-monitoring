// Copyright (c) 2022 Jonathan Lang

using System;
using UnityEngine.Scripting;

namespace Baracuda.Monitoring
{
    [Preserve]
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Event | AttributeTargets.Method | AttributeTargets.Class)]
    public class MOptionsAttribute : MonitoringMetaAttribute
    {
        /*
         * Formatting Options   
         */
        
        /// <summary>
        /// Custom format string used to display the members value if possible.
        /// </summary>
        public string Format { get; set; }
        
        /// <summary>
        /// Custom label for the member. 
        /// </summary>
        public string Label { get; set; } = null;
        
        /// <summary>
        /// Set the fontsize for the UI.
        /// </summary>
        public int FontSize { get; set; } = -1;
        
        /// <summary>
        /// Pass the name of a custom font style that will be used for the target member.
        /// FontName assets must be registered to the UI Controller.
        /// </summary>
        public string FontName { get; set; }
        
        /// <summary>
        /// Set the group for the element.
        /// </summary>
        public string GroupName { get; set; }
        
        /// <summary>
        /// Whether or not the unit should be wrapped in an object or type group.
        /// </summary>
        public bool GroupElement { get; set; } = true;
        
        /// <summary>
        /// If the member is a collection, determine if the index of individual elements should be displayed or not.
        /// </summary>
        public bool ShowIndexer { get; set; } = true;
        
        /// <summary>
        /// The indent of individual elements of a displayed collection.
        /// This property will only have an effect on collections.
        /// </summary>
        public int ElementIndent { get; set; } = -1;
        
        /// <summary>
        /// The preferred position of an individual UIElement on the canvas. 
        /// </summary>
        public UIPosition Position { get; set; } = UIPosition.UpperLeft;

        /// <summary>
        /// Horizontal Text Align
        /// </summary>
        public HorizontalTextAlign TextAlign { get; set; } = HorizontalTextAlign.Left;
        
        /// <summary>
        /// Relative vertical order of the displayed element.
        /// </summary>
        public int Order { get; set; }
        
        /// <summary>
        /// Relative vertical order of the displayed element.
        /// </summary>
        public int GroupOrder { get; set; }

        /// <summary>
        /// Override local RichText settings.
        /// </summary>
        public bool RichText { get; set; } = true;
        
        /// <summary>
        /// Determine the text color for the displayed value.
        /// </summary>
        public string TextColor { get; set; }
        
        /// <summary>
        /// Determine the background color for the displayed value.
        /// </summary>
        public string BackgroundColor { get; set; }
        
        /// <summary>
        /// Determine the background color for the group of the displayed value.
        /// </summary>
        public string GroupColor { get; set; }

        /// <summary>
        /// Set optional tags for the monitored member.
        /// </summary>
        public string[] Tags { get; set; } = Array.Empty<string>();

        /*
         * Monitoring Options   
         */
        
        /// <summary>
        /// Set a method as a custom value processor for a monitored member.
        /// The method must return a string and accept the value of the monitored member.
        /// </summary>
        /// <footer>Note: use the nameof keyword to pass the name of the method.</footer> 
        public string ValueProcessor { get; set; }
        
        /// <summary>
        /// The name of an event that is invoked when the monitored value is updated. Use to reduce the evaluation of the
        /// monitored member. Events can be of type <see cref="Action"/> or <see cref="Action{T}"/>, with T being the type of the monitored value. 
        /// </summary>
        /// <footer>Note: use the nameof keyword to pass the name of the method.</footer> 
        public string UpdateEvent { get; set; }

        /// <summary>
        /// Set the default enabled state for the monitored member.
        /// </summary>
        public bool Enabled { get; set; } = true;

        //--------------------------------------------------------------------------------------------------------------
        
        public MOptionsAttribute(string format)
        {
            Format = format;
        }
        
        public MOptionsAttribute(UIPosition position)
        {
            Position = position;
        }
        
        public MOptionsAttribute(string[] tags)
        {
            Tags = tags;
        }

        public MOptionsAttribute()
        {
        }
    }
    
    /*
     * Custom Enums
     */

    public enum UIPosition
    {
        UpperLeft = 0,
        UpperRight = 1,
        LowerLeft = 2,
        LowerRight = 3,
    }
    
    public enum HorizontalTextAlign
    {
        Left = 0,
        Center = 1,
        Right = 2
    }

    public static class EnumExtensions
    {
        public static string AsString(this UIPosition position)
        {
            switch (position)
            {
                case UIPosition.UpperLeft:
                    return nameof(UIPosition.UpperLeft);
                case UIPosition.UpperRight:
                    return nameof(UIPosition.UpperRight);
                case UIPosition.LowerLeft:
                    return nameof(UIPosition.LowerLeft);
                case UIPosition.LowerRight:
                    return nameof(UIPosition.LowerRight);
                default:
                    throw new ArgumentOutOfRangeException(nameof(position));
            }
        }
    }
}
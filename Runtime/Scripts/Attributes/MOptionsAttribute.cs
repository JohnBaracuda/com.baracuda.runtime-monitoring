// Copyright (c) 2022 Jonathan Lang

using System;
using UnityEngine.Scripting;

namespace Baracuda.Monitoring
{
    /// <summary>
    /// Attribute contains multiple settings.
    /// </summary>
    [Preserve]
    [AttributeUsage(Targets)]
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
        public bool ShowIndex { get; set; } = true;

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

        /// <summary>
        /// Attribute contains multiple settings.
        /// </summary>
        public MOptionsAttribute(string format)
        {
            Format = format;
        }

        /// <summary>
        /// Attribute contains multiple settings.
        /// </summary>
        public MOptionsAttribute(UIPosition position)
        {
            Position = position;
        }

        /// <summary>
        /// Attribute contains multiple settings.
        /// </summary>
        public MOptionsAttribute(string[] tags)
        {
            Tags = tags;
        }

        /// <summary>
        /// Attribute contains multiple settings.
        /// </summary>
        public MOptionsAttribute()
        {
        }

        //--------------------------------------------------------------------------------------------------------------

        #region Obsolete


        [Obsolete("Use ShowIndex instead! This API will be removed in 4.0.0")]
        public bool ShowIndexer
        {
            get => ShowIndex;
            set => ShowIndex = value;
        }

        #endregion
    }
}
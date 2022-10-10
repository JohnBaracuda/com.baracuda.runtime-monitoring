// Copyright (c) 2022 Jonathan Lang

using System;
using UnityEngine;

namespace Baracuda.Monitoring
{
    /// <summary>
    /// Interface grants access to various formatting options for a monitored member.
    /// </summary>
    public interface IFormatData
    {
        /// <summary>
        /// The format string for the member.
        /// </summary>
        string Format { get; }

        /// <summary>
        /// When enabled, the index of the elements of a monitored collection are displayed.
        /// </summary>
        bool ShowIndex { get; }

        /// <summary>
        /// The label for the monitored member.
        /// </summary>
        string Label { get; }

        /// <summary>
        /// The font size for the monitored member.
        /// </summary>
        int FontSize { get; }

        /// <summary>
        /// The font name for the monitored member.
        /// </summary>
        string FontName { get; }

        /// <summary>
        /// The hash code of the font for the monitored member.
        /// </summary>
        int FontHash { get; }

        /// <summary>
        /// The UI position for the monitored member.
        /// </summary>
        UIPosition Position { get; }

        /// <summary>
        /// The horizontal align for the monitored member.
        /// </summary>
        HorizontalTextAlign TextAlign { get; }

        /// <summary>
        /// Allow UI grouping for the monitored members display.
        /// </summary>
        bool AllowGrouping { get; }

        /// <summary>
        /// The UI group for the monitored member.
        /// </summary>
        string Group { get; }

        /// <summary>
        /// The element indent for the monitored member.
        /// </summary>
        int ElementIndent { get; }

        /// <summary>
        /// The RichText enabled settings for the monitored member.
        /// </summary>
        bool RichTextEnabled { get; }

        /// <summary>
        /// The order for the monitored member.
        /// </summary>
        int Order { get; }

        /// <summary>
        /// The group order for the monitored member.
        /// </summary>
        int GroupOrder { get; }

        /// <summary>
        /// The text color for the monitored member.
        /// </summary>
        Color? TextColor { get; }

        /// <summary>
        /// The background color for the monitored member.
        /// </summary>
        Color? BackgroundColor { get; }

        /// <summary>
        /// The group color for the monitored member.
        /// </summary>
        Color? GroupColor { get; }

        #region Obsolete

        [Obsolete("Use ShowIndex instead! This API will be removed in 4.0.0")]
        bool ShowIndexer { get; }

        #endregion
    }
}
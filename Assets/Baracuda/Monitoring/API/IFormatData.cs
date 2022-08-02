// Copyright (c) 2022 Jonathan Lang

using UnityEngine;

namespace Baracuda.Monitoring.API
{
    public interface IFormatData
    {
        string Format { get; }
        bool ShowIndexer { get; }
        string Label { get; }
        int FontSize { get; }
        string FontName { get; }
        int FontHash { get; }
        UIPosition Position { get; }
        HorizontalTextAlign TextAlign { get; }
        bool AllowGrouping { get; }
        string Group { get; }
        int ElementIndent { get; }
        bool RichTextEnabled { get; }
        int Order { get; }
        int GroupOrder { get; }
        Color? TextColor { get; }
        Color? BackgroundColor { get; }
        Color? GroupColor { get; }
    }
}
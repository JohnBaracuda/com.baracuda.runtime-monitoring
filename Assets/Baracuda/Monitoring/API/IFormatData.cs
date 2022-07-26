// Copyright (c) 2022 Jonathan Lang
 
namespace Baracuda.Monitoring.API
{
    public interface IFormatData
    {
        string Format { get; }
        bool ShowIndexer { get; }
        string Label { get; }
        int FontSize { get; }
        UIPosition Position { get; }
        HorizontalTextAlign TextAlign { get; }
        int ElementIndent { get; }
        bool AllowGrouping { get; }
        string Group { get; }
    }
}
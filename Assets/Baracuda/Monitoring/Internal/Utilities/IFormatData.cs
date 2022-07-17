using System;

namespace Baracuda.Monitoring.Internal.Utilities
{
    public interface IFormatData
    {
        string Format { get; }
        bool ShowIndexer { get; }
        string Label { get; }
        int FontSize { get; }
        UIPosition Position { get; }
        int ElementIndent { get; }
        bool AllowGrouping { get; }
        string Group { get; }
    }
}
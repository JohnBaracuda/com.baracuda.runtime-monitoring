// Copyright (c) 2022 Jonathan Lang

using Baracuda.Monitoring.API;
using Baracuda.Monitoring.Source.Profiles;
using Baracuda.Reflection;

namespace Baracuda.Monitoring.Source.Utilities
{
    public class FormatData : IFormatData
    {
        public string Format { get; internal set; }
        public bool ShowIndexer { get; internal set; }
        public string Label { get; internal set; }
        public int FontSize { get; internal set; }
        public UIPosition Position { get; internal set; }
        public HorizontalTextAlign TextAlign { get; internal set; }
        public int ElementIndent { get; internal set; }
        public bool AllowGrouping { get; internal set; }
        public string Group { get; internal set; }
        
        /*
         * Ctor   
         */
    
        internal FormatData()
        {
        }

        /*
         * Factory   
         */
        
        internal static IFormatData CreateFormatData(IMonitorProfile profile, IMonitoringSettings settings)
        {
            var formatAttribute = profile.GetMetaAttribute<MFormatOptionsAttribute>();

            var format = formatAttribute?.Format;
            var showIndexer = formatAttribute?.ShowIndexer ?? true;
            var label = formatAttribute?.Label;
            var fontSize = formatAttribute?.FontSize ?? -1;
            var position = formatAttribute?.Position ?? UIPosition.UpperLeft;
            var align = formatAttribute?.TextAlign ?? HorizontalTextAlign.Left;
            var allowGrouping = formatAttribute?.GroupElement ?? true;
            var indent = formatAttribute?.ElementIndent ?? -1;
            var group = settings.HumanizeNames? profile.UnitTargetType.Name.Humanize() : profile.UnitTargetType.Name;
            
            if (profile.UnitTargetType.IsGenericType)
            {
                group = profile.UnitTargetType.ToReadableTypeString();
            }
            
            if (label == null)
            {
                label = settings.HumanizeNames? profile.MemberInfo.Name.Humanize(settings.VariablePrefixes) : profile.MemberInfo.Name;
                
                if (settings.AddClassName)
                {
                    label = $"{profile.UnitTargetType.Name.Colorize(settings.ClassColor)}{settings.AppendSymbol.ToString()}{label}";
                }
            }

            return new FormatData
            {
                Format = format,
                ShowIndexer = showIndexer,
                Label = label,
                FontSize = fontSize,
                Position = position,
                TextAlign = align,
                AllowGrouping = allowGrouping,
                ElementIndent = indent,
                Group = group
            };
        }
    }
}

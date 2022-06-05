// Copyright (c) 2022 Jonathan Lang
using Baracuda.Monitoring.API;
using Baracuda.Monitoring.Internal.Profiling;
using Baracuda.Reflection;

namespace Baracuda.Monitoring.Internal.Utilities
{
    public class FormatData 
    {
        public string Format { get; }
        public bool ShowIndexer { get; }
        public string Label { get; }
        public int FontSize { get; }
        public UIPosition Position { get; }
        
        public bool AllowGrouping { get; }
        public string Group { get; }

        /*
         * Ctor   
         */
                
        private FormatData(string format, bool showIndexer, string label, int fontSize, UIPosition position, bool allowGrouping, string group)
        {
            Format = format;
            ShowIndexer = showIndexer;
            Label = label;
            FontSize = fontSize;
            Position = position;
            AllowGrouping = allowGrouping;
            Group = group;
        }

        /*
         * Factory   
         */
        
        internal static FormatData Create(MonitorProfile profile, MonitoringSettings settings)
        {
            var formatAttribute = profile.GetMetaAttribute<MFormatOptionsAttribute>();

            var format = formatAttribute?.Format;
            var showIndexer = formatAttribute?.ShowIndexer ?? true;
            var label = formatAttribute?.Label;
            var fontSize = formatAttribute?.FontSize ?? -1;
            var position = formatAttribute?.Position ?? UIPosition.UpperLeft;
            var allowGrouping = formatAttribute?.GroupElement ?? true;
            var group = settings.HumanizeNames? profile.UnitTargetType.Name.Humanize() : profile.UnitTargetType.Name;
            
            if (profile.UnitTargetType.IsGenericType)
            {
                group = profile.UnitTargetType.ToSyntaxString();
            }
            
            if (label == null)
            {
                label = settings.HumanizeNames? profile.MemberInfo.Name.Humanize(settings.VariablePrefixes) : profile.MemberInfo.Name;
                
                if (settings.AddClassName)
                {
                    label = $"{profile.UnitTargetType.Name.Colorize(settings.ClassColor)}{settings.AppendSymbol.ToString()}{label}";
                }
            }
            
            return new FormatData(format, showIndexer, label, fontSize, position, allowGrouping, group);
        }
    }
}

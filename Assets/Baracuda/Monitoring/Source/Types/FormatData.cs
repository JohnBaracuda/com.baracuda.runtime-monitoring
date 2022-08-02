// Copyright (c) 2022 Jonathan Lang

using Baracuda.Monitoring.API;
using Baracuda.Reflection;
using UnityEngine;

namespace Baracuda.Monitoring.Source.Types
{
    internal class FormatData : IFormatData
    {
        public string Format { get; internal set; }
        public bool ShowIndexer { get; internal set; }
        public string Label { get; internal set; }
        public int FontSize { get; internal set; }
        public string FontName { get; internal set; }
        public int FontHash { get; internal set; }
        public UIPosition Position { get; internal set; }
        public HorizontalTextAlign TextAlign { get; internal set; }
        public bool AllowGrouping { get; internal set; }
        public string Group { get; internal set; }
        public int GroupOrder { get; internal set; }
        public int ElementIndent { get; internal set; }
        public bool RichTextEnabled { get; internal set; } = true;
        public int Order { get; internal set; }
        public Color? TextColor { get; internal set; }
        public Color? BackgroundColor { get; internal set; }
        public Color? GroupColor { get; internal set; }
        
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
            var optionsAttribute = profile.GetMetaAttribute<MOptionsAttribute>();

            var format = profile.TryGetMetaAttribute<MFormatAttribute>(out var formatAttribute)
                ? formatAttribute.Format
                : optionsAttribute?.Format;
            
            var showIndexer = profile.TryGetMetaAttribute<MShowIndexerAttribute>(out var showIndexerAttribute)
                ? showIndexerAttribute.ShowIndexer
                : optionsAttribute?.ShowIndexer ?? true;
            
            var label = profile.TryGetMetaAttribute<MLabelAttribute>(out var labelAttribute)
                ? labelAttribute.Label
                : optionsAttribute?.Label ?? MakeLabel();
            
            var fontSize = profile.TryGetMetaAttribute<MFontSizeAttribute>(out var fontSizeAttribute)
                ? fontSizeAttribute.FontSize
                : optionsAttribute?.FontSize ?? -1;
            
            var fontName = profile.TryGetMetaAttribute<MFontNameAttribute>(out var fontNameAttribute)
                ? fontNameAttribute.FontName
                : optionsAttribute?.FontName;
            
            var position = profile.TryGetMetaAttribute<MPositionAttribute>(out var positionAttribute)
                ? positionAttribute.Position
                : optionsAttribute?.Position ?? UIPosition.UpperLeft;
            
            var align = profile.TryGetMetaAttribute<MTextAlignAttribute>(out var textAlignAttribute)
                ? textAlignAttribute.TextAlign
                : optionsAttribute?.TextAlign ?? HorizontalTextAlign.Left;
            
            var allowGrouping = profile.TryGetMetaAttribute<MGroupElementAttribute>(out var groupElementAttribute)
                ? groupElementAttribute.GroupElement
                : optionsAttribute?.GroupElement ?? true;

            var group = profile.TryGetMetaAttribute<MGroupNameAttribute>(out var groupNameAttribute)
                ? groupNameAttribute.GroupName
                : optionsAttribute?.GroupName ?? (profile.IsStatic ? MakeGroup() : null);
            
            var elementIndent = profile.TryGetMetaAttribute<MElementIndentAttribute>(out var elementIndentAttribute)
                ? elementIndentAttribute.ElementIndent
                : optionsAttribute?.ElementIndent ?? -1;
            
            var richText = profile.TryGetMetaAttribute<MRichTextAttribute>(out var richTextAttribute)
                ? richTextAttribute.RichTextEnabled
                : optionsAttribute?.RichText ?? true;
            
            var order = profile.TryGetMetaAttribute<MOrderAttribute>(out var orderAttribute)
                ? orderAttribute.Order
                : optionsAttribute?.Order ?? 0;
            
            var groupOrder = profile.TryGetMetaAttribute<MGroupOrderAttribute>(out var groupOrderAttribute)
                ? groupOrderAttribute.Order
                : optionsAttribute?.Order ?? 0;

            var textColor = profile.TryGetMetaAttribute<MTextColorAttribute>(out var textColorAttribute)
                ? textColorAttribute.ColorValue
                : MakeColor(optionsAttribute?.TextColor);

            var backgroundColor = profile.TryGetMetaAttribute<MBackgroundColorAttribute>(out var backgroundColorAttribute)
                ? backgroundColorAttribute.ColorValue
                : MakeColor(optionsAttribute?.BackgroundColor);
            
            var groupColor = profile.TryGetMetaAttribute<MGroupColorAttribute>(out var groupColorAttribute)
                ? groupColorAttribute.ColorValue
                : MakeColor(optionsAttribute?.GroupColor);
            
            
            return new FormatData
            {
                Format = format,
                ShowIndexer = showIndexer,
                Label = label,
                FontSize = fontSize,
                FontName = fontName,
                FontHash = fontName?.GetHashCode() ?? 0,
                Position = position,
                TextAlign = align,
                AllowGrouping = allowGrouping,
                Group = group,
                ElementIndent = elementIndent,
                RichTextEnabled = richText,
                Order = order,
                GroupOrder = groupOrder,
                TextColor = textColor,
                BackgroundColor = backgroundColor,
                GroupColor = groupColor
            };
            
            //Nested
            
                        
            Color? MakeColor(string colorString)
            {
                if (colorString != null && ColorUtility.TryParseHtmlString(colorString, out var color))
                {
                    return color;
                }
                return null;
            }

            string MakeGroup()
            {
                return profile.DeclaringType.IsGenericType
                    ? profile.DeclaringType.ToReadableTypeString()
                    : (settings.HumanizeNames
                        ? profile.DeclaringType.Name.Humanize()
                        : profile.DeclaringType.Name);
            }
            
            string MakeLabel()
            {
                var humanizedLabel = settings.HumanizeNames? profile.MemberInfo.Name.Humanize(settings.VariablePrefixes) : profile.MemberInfo.Name;
                
                if (settings.AddClassName)
                {
                    humanizedLabel = $"{profile.DeclaringType.Name.Colorize(settings.ClassColor)}{settings.AppendSymbol.ToString()}{humanizedLabel}";
                }

                return humanizedLabel;
            }
        }
    }
}

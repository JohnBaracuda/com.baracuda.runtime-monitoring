// Copyright (c) 2022 Jonathan Lang

using Baracuda.Monitoring.Utilities.Extensions;
using Baracuda.Monitoring.Utilities.Pooling;
using System;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Baracuda.Monitoring.Types
{
    internal class FormatData : IFormatData
    {
        public string Format { get; internal set; }
        public bool ShowIndex { get; internal set; }
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

            var showIndexer = profile.TryGetMetaAttribute<MShowIndexAttribute>(out var showIndexerAttribute)
                ? showIndexerAttribute.ShowIndex
                : optionsAttribute?.ShowIndex ?? true;

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
                : optionsAttribute?.GroupOrder ?? 0;

            var textColor = profile.TryGetMetaAttribute<MTextColorAttribute>(out var textColorAttribute)
                ? textColorAttribute.ColorValue
                : MakeColor(optionsAttribute?.TextColor);

            var backgroundColor =
                profile.TryGetMetaAttribute<MBackgroundColorAttribute>(out var backgroundColorAttribute)
                    ? backgroundColorAttribute.ColorValue
                    : MakeColor(optionsAttribute?.BackgroundColor);

            var groupColor = profile.TryGetMetaAttribute<MGroupColorAttribute>(out var groupColorAttribute)
                ? groupColorAttribute.ColorValue
                : MakeColor(optionsAttribute?.GroupColor);


            return new FormatData
            {
                Format = format,
                ShowIndex = showIndexer,
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
                    ? profile.DeclaringType.HumanizedName()
                    : (settings.HumanizeNames
                        ? Humanize(profile.DeclaringType.Name)
                        : profile.DeclaringType.Name);
            }

            string MakeLabel()
            {
                var humanizedLabel = settings.HumanizeNames
                    ? Humanize(profile.MemberInfo.Name, settings.VariablePrefixes)
                    : profile.MemberInfo.Name;

                if (settings.AddClassName)
                {
                    humanizedLabel =
                        $"{profile.DeclaringType.Name.ColorizeString(settings.ClassColor)}{settings.AppendSymbol.ToString()}{humanizedLabel}";
                }

                return humanizedLabel;
            }
        }

        private static string Humanize(string target, string[] prefixes = null)
        {
            if (IsConstantStringSyntax(target))
            {
                return ToCamel(target.Replace('_', ' '));
            }

            if (prefixes != null)
            {
                for (var i = 0; i < prefixes.Length; i++)
                {
                    if (target.StartsWith(prefixes[i]))
                    {
                        target = target.Replace(prefixes[i], string.Empty);
                    }
                }
            }

            target = target.Replace('_', ' ');

            var chars = ListPool<char>.Get();

            for (var i = 0; i < target.Length; i++)
            {
                if (i == 0)
                {
                    chars.Add(char.ToUpper(target[i]));
                    continue;
                }

                if (i < target.Length - 1)
                {
                    if (char.IsUpper(target[i]) && !char.IsUpper(target[i + 1])
                        || char.IsUpper(target[i]) && !char.IsUpper(target[i - 1]))
                    {
                        if (i > 1)
                        {
                            chars.Add(' ');
                        }
                    }
                }

                chars.Add(target[i]);
            }

            var array = chars.ToArray();
            ListPool<char>.Release(chars);
            return ReduceWhitespace(new string(array));

            string ReduceWhitespace(string value)
            {
                var sb = ConcurrentStringBuilderPool.Get();
                var previousIsWhitespaceFlag = false;
                for (var i = 0; i < value.Length; i++)
                {
                    if (char.IsWhiteSpace(value[i]))
                    {
                        if (previousIsWhitespaceFlag)
                        {
                            continue;
                        }

                        previousIsWhitespaceFlag = true;
                    }
                    else
                    {
                        previousIsWhitespaceFlag = false;
                    }

                    sb.Append(value[i]);
                }

                return ConcurrentStringBuilderPool.Release(sb);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static string ToCamel(string content)
        {
            var sb = ConcurrentStringBuilderPool.Get();

            for (var i = 0; i < content.Length; i++)
            {
                var current = content[i];
                var last = i > 0 ? content[i - 1] : ' ';
                sb.Append(last == ' ' ? char.ToUpper(current) : char.ToLower(current));
            }

            return ConcurrentStringBuilderPool.Release(sb);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool IsConstantStringSyntax(string input)
        {
            for (var i = 0; i < input.Length; i++)
            {
                var character = input[i];
                if (!char.IsUpper(character) && character != '_')
                {
                    return false;
                }
            }

            return true;
        }

        #region Obsolete

        [Obsolete("Use ShowIndex instead! This API will be removed in 4.0.0")]
        public bool ShowIndexer => ShowIndex;

        #endregion
    }
}
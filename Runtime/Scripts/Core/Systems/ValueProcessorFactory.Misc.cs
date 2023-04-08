// Copyright (c) 2022 Jonathan Lang

using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Baracuda.Monitoring.Systems
{
    internal partial class ValueProcessorFactory
    {
        /*
         * Fields
         */

        private static readonly Dictionary<int, string> indentStringCache = new Dictionary<int, string>();

        /*
         * Methods
         */

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static string GetIndentStringForProfile(IFormatData formatData)
        {
            var indent = CreateIndentValueForProfile(formatData);
            return GetIndentStringForValue(indent);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int CreateIndentValueForProfile(IFormatData formatData)
        {
            return formatData.ElementIndent >= 0 ? formatData.ElementIndent : DefaultIndentNum;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static string GetIndentStringForValue(int indent)
        {
            if (indentStringCache.TryGetValue(indent, out var result))
            {
                return result;
            }

            result = new string(' ', indent);
            indentStringCache.Add(indent, result);
            return result;
        }
    }
}
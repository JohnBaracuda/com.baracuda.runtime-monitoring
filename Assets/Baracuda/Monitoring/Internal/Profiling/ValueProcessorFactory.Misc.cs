// Copyright (c) 2022 Jonathan Lang
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Baracuda.Monitoring.Internal.Profiling
{
    internal static partial class ValueProcessorFactory
    {
        /*
         * Fields   
         */
        
        private static readonly Dictionary<int, string> indentStringCache = new Dictionary<int, string>();

        /*
         * Methods   
         */
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static string GetIndentStringForProfile(MonitorProfile profile)
        {
            var indent = CreateIndentValueForProfile(profile);
            return GetIndentStringForValue(indent);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int CreateIndentValueForProfile(MonitorProfile profile)
        {
            return profile.TryGetMetaAttribute<MFormatOptionsAttribute>(out var attribute)
                ? attribute.ElementIndent >= 0 ? attribute.ElementIndent : DEFAULT_INDENT_NUM
                : DEFAULT_INDENT_NUM;
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
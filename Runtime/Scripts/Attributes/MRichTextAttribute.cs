// Copyright (c) 2022 Jonathan Lang

using System;

namespace Baracuda.Monitoring
{
    /// <summary>
    /// Use to override local RichText settings.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Method | AttributeTargets.Class)]
    public class MRichTextAttribute : MonitoringMetaAttribute
    {
        /// <summary>
        /// Used to override local RichText enabled settings.
        /// </summary>
        public readonly bool RichTextEnabled;

        /// <summary>
        /// Use to override local RichText settings.
        /// </summary>
        public MRichTextAttribute(bool enabled)
        {
            RichTextEnabled = enabled;
        }
    }
}
// Copyright (c) 2022 Jonathan Lang

using System;
using UnityEngine.Scripting;

namespace Baracuda.Monitoring
{
    /// <summary>
    /// Add tags to a monitored unit that can be used to provide additional meta data and filtering options for UI.
    /// </summary>
    [Preserve]
    [AttributeUsage(Targets)]
    public class MTagAttribute : MonitoringMetaAttribute
    {
        /// <summary>
        /// Collection of passed tags.
        /// </summary>
        public string[] Tags { get; }

        /// <summary>
        /// Add tags to a monitored unit that can be used to provide additional meta data and filtering options for UI.
        /// </summary>
        public MTagAttribute(string tag)
        {
            Tags = new[] {tag};
        }

        /// <summary>
        /// Add tags to a monitored unit that can be used to provide additional meta data and filtering options for UI.
        /// </summary>
        public MTagAttribute(string tag1, string tag2)
        {
            Tags = new[] {tag1, tag2};
        }

        /// <summary>
        /// Add tags to a monitored unit that can be used to provide additional meta data and filtering options for UI.
        /// </summary>
        public MTagAttribute(string tag1, string tag2, string tag3)
        {
            Tags = new[] {tag1, tag2, tag3};
        }

        /// <summary>
        /// Add tags to a monitored unit that can be used to provide additional meta data and filtering options for UI.
        /// </summary>
        public MTagAttribute(params string[] tags)
        {
            Tags = tags;
        }
    }
}
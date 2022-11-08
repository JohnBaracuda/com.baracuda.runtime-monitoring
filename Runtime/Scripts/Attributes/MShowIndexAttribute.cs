// Copyright (c) 2022 Jonathan Lang

using System;
using UnityEngine.Scripting;

namespace Baracuda.Monitoring
{
    /// <summary>
    /// If the member is a collection, determine if the index of individual elements should be displayed or not.
    /// </summary>
    [Preserve]
    [AttributeUsage(Targets)]
    public class MShowIndexAttribute : MonitoringMetaAttribute
    {
        /// <summary>
        /// If the member is a collection, determine if the index of individual elements should be displayed or not.
        /// </summary>
        public readonly bool ShowIndex;

        /// <summary>
        /// If the member is a collection, determine if the index of individual elements should be displayed or not.
        /// </summary>
        public MShowIndexAttribute(bool showIndex)
        {
            ShowIndex = showIndex;
        }
    }

    [Obsolete("Use MShowIndexAttribute instead! This class will be removed in 4.0.0")]
    [AttributeUsage(Targets)]
    public class MShowIndexerAttribute : MShowIndexAttribute
    {
        [Obsolete("Use MShowIndexAttribute instead! This class will be removed in 4.0.0")]
        public bool ShowIndexer => ShowIndex;

        [Obsolete("Use MShowIndexAttribute instead! This class will be removed in 4.0.0")]
        public MShowIndexerAttribute(bool showIndex) : base(showIndex)
        {
        }
    }
}
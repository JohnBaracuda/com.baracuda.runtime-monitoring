// Copyright (c) 2022 Jonathan Lang

using System;
using UnityEngine.Scripting;

namespace Baracuda.Monitoring
{
    [Preserve]
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Event | AttributeTargets.Method | AttributeTargets.Class)]
    public class MShowIndexerAttribute : MonitoringMetaAttribute
    {
        /// <summary>
        /// If the member is a collection, determine if the index of individual elements should be displayed or not.
        /// </summary>
        public readonly bool ShowIndexer;

        /// <summary>
        /// If the member is a collection, determine if the index of individual elements should be displayed or not.
        /// </summary>
        public MShowIndexerAttribute(bool showIndexer)
        {
            ShowIndexer = showIndexer;
        }
    }
}
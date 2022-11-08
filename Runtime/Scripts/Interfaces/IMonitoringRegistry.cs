// Copyright (c) 2022 Jonathan Lang

using System;
using System.Collections.Generic;

namespace Baracuda.Monitoring
{
    public interface IMonitoringRegistry
    {
        /// <summary>
        /// Get a list of monitoring handles for all targets.
        /// </summary>
        IReadOnlyList<IMonitorHandle> GetMonitorHandles(HandleTypes handleTypes = HandleTypes.All);

        /// <summary>
        /// Get a list of <see cref="IMonitorHandle"/>s registered to the passed target object.
        /// </summary>
        IMonitorHandle[] GetMonitorHandlesForTarget<T>(T target) where T : class;

        /// <summary>
        /// Get a collection of used tags.
        /// </summary>
        IReadOnlyList<string> UsedTags { get; }

        /// <summary>
        /// Get a collection of used font names.
        /// </summary>
        IReadOnlyList<string> UsedFonts { get; }

        /// <summary>
        /// Get a collection of monitored types.
        /// </summary>
        IReadOnlyList<Type> UsedTypes { get; }

        /// <summary>
        /// Get a collection of monitored type names converted to a readable string.
        /// </summary>
        IReadOnlyList<string> UsedTypeNames { get; }
    }
}
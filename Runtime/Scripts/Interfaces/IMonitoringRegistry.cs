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
        IReadOnlyCollection<string> UsedTags { get; }

        /// <summary>
        /// Get a collection of used fonts.
        /// </summary>
        IReadOnlyCollection<string> UsedFonts { get; }

        /// <summary>
        /// Get a collection of monitored types.
        /// </summary>
        IReadOnlyCollection<Type> UsedTypes { get; }
    }
}
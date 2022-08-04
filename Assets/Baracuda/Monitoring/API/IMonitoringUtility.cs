// Copyright (c) 2022 Jonathan Lang

using System.Collections.Generic;
using JetBrains.Annotations;

namespace Baracuda.Monitoring.API
{
    public interface IMonitoringUtility : IMonitoringSubsystem<IMonitoringUtility>
    {
        /// <summary>
        /// Method returns true if the passed hash from the name of a font asset is used by a MFontNameAttribute and therefore
        /// required by a monitoring unit. Used to dynamically load/unload required fonts.
        /// </summary>
        /// <param name="fontHash">The hash of the fonts name (string)</param>
        [Pure] bool IsFontHashUsed(int fontHash);

        /// <summary>
        /// Get a list of <see cref="IMonitorUnit"/>s registered to the passed target object. 
        /// </summary>
        [Pure] IMonitorUnit[] GetMonitorUnitsForTarget(object target);

        /// <summary>
        /// Get a list of all custom tags, applied by [MTag] attributes that can be used for filtering. 
        /// </summary>
        [Pure] IReadOnlyCollection<string> GetAllTags();
        
        [Pure] IReadOnlyCollection<string> GetAllTypeStrings();
    }
}
// Copyright (c) 2022 Jonathan Lang

using System;
using System.Collections.Generic;

namespace Baracuda.Monitoring
{
    [Obsolete("Use Baracuda.Monitor.Registry instead! This API will be removed in 4.0.0")]
    public interface IMonitoringUtility : IMonitoringSubsystem<IMonitoringUtility>
    {
        [Obsolete("Use Baracuda.Monitor.Registry.UsedFonts instead! This API will be removed in 4.0.0")]
        bool IsFontHashUsed(int fontHash);

        [Obsolete("Use Baracuda.Monitor.Registry.GetMonitorHandlesForTarget instead! This API will be removed in 4.0.0")]
        IMonitorHandle[] GetMonitorUnitsForTarget(object target);

        [Obsolete("Use Baracuda.Monitor.Registry.UsedTags instead! This API will be removed in 4.0.0")]
        IReadOnlyCollection<string> GetAllTags();

        [Obsolete("Use Baracuda.Monitor.Registry.UsedTypes instead! This API will be removed in 4.0.0")]
        IReadOnlyCollection<string> GetAllTypeStrings();
    }
}
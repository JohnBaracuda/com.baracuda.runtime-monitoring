// Copyright (c) 2022 Jonathan Lang

using Baracuda.Monitoring.Utilities.Extensions;
using System;
using System.Collections.Generic;

namespace Baracuda.Monitoring.Systems
{
    [Obsolete]
    internal class MonitoringUtility : IMonitoringUtility
    {
        [Obsolete]
        public bool IsFontHashUsed(int fontHash)
        {
            foreach (var registryUsedFont in Monitor.Registry.UsedFonts)
            {
                if (registryUsedFont.GetHashCode() == fontHash)
                {
                    return true;
                }
            }
            return false;
        }

        [Obsolete]
        public IMonitorHandle[] GetMonitorUnitsForTarget(object target)
        {
            return Monitor.Registry.GetMonitorHandlesForTarget(target);
        }

        [Obsolete]
        public IReadOnlyCollection<string> GetAllTags()
        {
            return Monitor.Registry.UsedTags;
        }

        [Obsolete]
        public IReadOnlyCollection<string> GetAllTypeStrings()
        {
            var set = new HashSet<string>();
            foreach (var registryUsedType in Monitor.Registry.UsedTypes)
            {
                set.Add(registryUsedType.HumanizedName());
            }
            return set;
        }
    }
}
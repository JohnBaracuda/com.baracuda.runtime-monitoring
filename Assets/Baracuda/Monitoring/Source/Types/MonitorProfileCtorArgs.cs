// Copyright (c) 2022 Jonathan Lang

using Baracuda.Monitoring.API;
using System.Reflection;

namespace Baracuda.Monitoring.Types
{
    /// <summary>
    /// Object acts as a wrapper for additional arguments that need to be passed when constructing a unit profile.
    /// </summary>
    public class MonitorProfileCtorArgs
    {
        public readonly BindingFlags ReflectedMemberFlags;
        public readonly IMonitoringSettings Settings;

        public MonitorProfileCtorArgs(BindingFlags reflectedMemberFlags, IMonitoringSettings settings)
        {
            ReflectedMemberFlags = reflectedMemberFlags;
            Settings = settings;
        }
    }
}
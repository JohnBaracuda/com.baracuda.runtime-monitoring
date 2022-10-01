// Copyright (c) 2022 Jonathan Lang

using Baracuda.Monitoring.Interfaces;
using System;
using System.Reflection;

namespace Baracuda.Monitoring.Core.Types
{
    /// <summary>
    /// Object acts as a wrapper for additional arguments that need to be passed when constructing a unit profile.
    /// </summary>
    internal class MonitorProfileCtorArgs
    {
        public readonly BindingFlags ReflectedMemberFlags;
        public readonly IMonitoringSettings Settings;
        public readonly Type DeclaringStruct;

        public MonitorProfileCtorArgs(BindingFlags reflectedMemberFlags, IMonitoringSettings settings, Type declaringStruct = null)
        {
            ReflectedMemberFlags = reflectedMemberFlags;
            Settings = settings;
            DeclaringStruct = declaringStruct;
        }
    }
}
// Copyright (c) 2022 Jonathan Lang

using System.Runtime.CompilerServices;
using Baracuda.Monitoring.API;

namespace Baracuda.Monitoring
{
    public static class MonitoringExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void RegisterMonitor(this object target)
        {
            MonitoringManager.RegisterTarget(target);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void UnregisterMonitor(this object target)
        {
            MonitoringManager.UnregisterTarget(target);
        }
    }
}
// Copyright (c) 2022 Jonathan Lang

using Baracuda.Monitoring.Systems;
using System;

namespace Baracuda.Monitoring
{
    [Obsolete("Use Baracuda.Monitoring.Monitor instead!")]
    public static class MonitoringSystems
    {
        [Obsolete("Use Monitor.Initialized instead! This API will be removed in 4.0.0")]
        public static bool Initialized => Monitor.Initialized;

        [Obsolete("Use Monitor, MonitoringEvents and MonitoringRegistry instead! This API will be removed in 4.0.0")]
        public static IMonitoringManager Manager { get; } = new MonitoringManager();

        [Obsolete("Use Monitor.Settings instead! This API will be removed in 4.0.0")]
        public static IMonitoringSettings Settings => Monitor.Settings;

        [Obsolete("Use Baracuda.Monitor.Registry instead! This API will be removed in 4.0.0")]
        public static IMonitoringUtility Utility { get; } = new MonitoringUtility();

        [Obsolete("Use Monitor.UI instead! This API will be removed in 4.0.0")]
        public static IMonitoringUI UI => Monitor.UI;


        [Obsolete("Use Baracuda.Monitoring.Monitor to access API instead! This API will be removed in 4.0.0")]
        public static T Resolve<T>() where T : class
        {
            if (typeof(T) == typeof(IMonitoringUI))
            {
                return (T) UI;
            }
            if (typeof(T) == typeof(IMonitoringSettings))
            {
                return (T) Settings;
            }
            if (typeof(T) == typeof(IMonitoringUtility))
            {
                return (T) Utility;
            }
            if (typeof(T) == typeof(IMonitoringManager))
            {
                return (T) Manager;
            }

            return null;
        }

        [Obsolete("Use Baracuda.Monitoring.Monitor to access API instead! This API will be removed in 4.0.0")]
        public static T Register<T>(T system) where T : class
        {
            return null;
        }

        [Obsolete("This exception is no longer used! This class will be removed in 4.0.0")]
        public class SystemNotRegisteredException : Exception
        {
            private SystemNotRegisteredException()
            {
            }
        }
    }
}
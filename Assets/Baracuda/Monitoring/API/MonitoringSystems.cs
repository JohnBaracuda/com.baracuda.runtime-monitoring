// Copyright (c) 2022 Jonathan Lang
 
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace Baracuda.Monitoring.API
{
    /// <summary>
    /// Class manages references for individual monitoring systems.
    /// </summary>
    public static class MonitoringSystems
    {
        private static readonly Dictionary<Type, object> systems = new Dictionary<Type, object>(8);

        [Pure]
        public static T Resolve<T>() where T : class, IMonitoringSystem<T>
        {
            return systems.TryGetValue(typeof(T), out var system) ? (T) system : throw new SystemNotRegisteredException(typeof(T).Name);
        }

        public static T Register<T>(T system) where T : class, IMonitoringSystem<T>
        {
            var key = typeof(T);
            if (systems.ContainsKey(key))
            {
                systems[key] = system;
            }
            else
            {
                systems.Add(key, system);
            }

            return system;
        }
        
        private class SystemNotRegisteredException : Exception
        {
            public SystemNotRegisteredException(string systemName) : base(
                $"System: [{systemName}] is not registered!") { }
        }
    }
}
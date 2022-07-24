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
#if CSHARP_8_OR_LATER
            return IMonitoringSystem<T>.Current ?? throw new SystemNotRegisteredException(typeof(T).Name);
#endif
            return systems.TryGetValue(typeof(T), out var system) ? (T) system : throw new SystemNotRegisteredException(typeof(T).Name);
        }

        public static void Register<T>(T system) where T : class, IMonitoringSystem<T>
        {
#if CSHARP_8_OR_LATER
            IMonitoringSystem<T>.Current = system;
#endif
            var key = typeof(T);
            if (systems.ContainsKey(key))
            {
                systems[key] = system;
            }
            else
            {
                systems.Add(key, system);
            }
        }
        
        private class SystemNotRegisteredException : Exception
        {
            public SystemNotRegisteredException(string systemName) : base(
                $"System: [{systemName}] is not registered!") { }
        }
    }
}
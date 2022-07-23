using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace Baracuda.Monitoring.API
{
    public interface IMonitoringService<T> where T : class, IMonitoringService<T>
    {
        
    }

    public static class MonitoringSystems
    {
        private static readonly Dictionary<Type, object> systems = new Dictionary<Type, object>(8);

        [Pure]
        public static T Resolve<T>() where T : class, IMonitoringService<T>
        {
            return systems.TryGetValue(typeof(T), out var system) ? (T) system : throw new NotImplementedException();
        }

        public static void Register<T>(T system)  where T : class, IMonitoringService<T>
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
        }
    }
}
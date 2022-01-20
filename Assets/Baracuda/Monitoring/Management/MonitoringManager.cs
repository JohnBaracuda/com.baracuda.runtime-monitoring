using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Baracuda.Monitoring.Internal.Profiling;
using Baracuda.Monitoring.Internal.Units;
using Baracuda.Pooling.Concretions;
using Baracuda.Reflection;
using Baracuda.Threading;

namespace Baracuda.Monitoring.Management
{
    public static class MonitoringManager
    {
        #region --- [COLLECTIONS] ---

        public static IReadOnlyList<MonitorUnit> GetStaticUnits => _staticUnits;
        public static IReadOnlyList<MonitorUnit> GetInstanceUnits => _instanceUnits;

        private static readonly List<MonitorUnit> _staticUnits = new List<MonitorUnit>(30);
        private static readonly List<MonitorUnit> _instanceUnits = new List<MonitorUnit>(30);
        
        private static readonly Dictionary<object, MonitorUnit[]> _activeInstanceUnits = new Dictionary<object, MonitorUnit[]>();
        
        private static bool _initialInstanceUnitsCreated = false;
        
        private static readonly List<object> _registeredTargets = new List<object>(300);
        
        #endregion
        
        //--------------------------------------------------------------------------------------------------------------
        
        #region --- [TARGET REGISTRATION] ---
        
        public static void RegisterTarget(object target)
        {
            _registeredTargets.Add(target);
            if (_initialInstanceUnitsCreated)
            {
                CreateInstanceUnits(target, target.GetType());
            }
        }
        
        public static void UnregisterTarget(object target)
        {
            DestroyInstanceUnits(target);
            _registeredTargets.Remove(target);
        }

        #endregion
        
        //--------------------------------------------------------------------------------------------------------------
        
        internal static async Task CompleteProfiling(CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            await CreateStaticUnits(MonitoringProfiler.StaticProfiles.ToArray());
            await Dispatcher.InvokeAsync(CreateInitialInstanceUnits, ct);
            await Dispatcher.InvokeAsync(() => MonitoringEvents.ProfilingCompletedInternal(_staticUnits.ToArray(), _instanceUnits.ToArray()), ct);
        }
        
        //--------------------------------------------------------------------------------------------------------------

        #region --- [INSTANTIATE: INSTANCE UNITS] ---
        
        private static void CreateInitialInstanceUnits()
        {
            for (var i = 0; i < _registeredTargets.Count; i++)
            {
                CreateInstanceUnits(_registeredTargets[i], _registeredTargets[i].GetType());
            }
            _initialInstanceUnitsCreated = true;
        }

        private static void CreateInstanceUnits(object target, Type type)
        {
            var validTypes = type.GetBaseTypes(true);
            // create a new array to cache the units instances that will be created. 
            var units = ConcurrentListPool<MonitorUnit>.Get();
            var guids = ConcurrentListPool<MemberInfo>.Get();
            
            for (var i = 0; i < validTypes.Length; i++)
            {
                if(validTypes[i].IsGenericType) continue;
                if (MonitoringProfiler.InstanceProfiles.TryGetValue(validTypes[i], out var profiles))
                {
                    // loop through the profiles and create a new unit for each profile.
                    for (var j = 0; j < profiles.Count; j++)
                    {
                        if(guids.Contains(profiles[j].MemberInfo)) continue;
                        guids.Add(profiles[j].MemberInfo);
                        
                        var unit = profiles[j].CreateUnit(target);
                        units.Add(unit);
                        _instanceUnits.Add(unit);
                        MonitoringEvents.RaiseUnitCreated(unit);
                    }
                }
            }

            // cache the created units in a dictionary that allows access by the units target.
            // this dictionary will be used to dispose the units if the target gets destroyed 
            if (units.Count > 0)
            {
                if (!_activeInstanceUnits.ContainsKey(target))
                {
                    _activeInstanceUnits.Add(target, units.ToArray());
                }
            }
            ConcurrentListPool<MemberInfo>.Release(guids);
            ConcurrentListPool<MonitorUnit>.Release(units);
        }

        #endregion

        #region --- [DISPOSE: INSTANCE UNITS] ---

        private static void DestroyInstanceUnits(object target)
        {
            if (_activeInstanceUnits.TryGetValue(target, out var units))
            {
                for (int i = 0; i < units.Length; i++)
                {
                    units[i].Dispose();
                    MonitoringEvents.RaiseUnitDisposed(units[i]);
                }
                
                _activeInstanceUnits.Remove(target);
            }
        }

        #endregion
        
        //--------------------------------------------------------------------------------------------------------------

        #region --- [INSTANTIATE: STATIC UNITS] ---

        private static async Task CreateStaticUnits(MonitorProfile[] staticProfiles)
        {
            for (var i = 0; i < staticProfiles.Length; i++)
            {
                await CreateStaticUnit(staticProfiles[i]);
            }
        }
        
        private static Task CreateStaticUnit(MonitorProfile staticProfile)
        {
            _staticUnits.Add(staticProfile.CreateUnit(null));
            return Task.CompletedTask;
        }

        #endregion
    }
}
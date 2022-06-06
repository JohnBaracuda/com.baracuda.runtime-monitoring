// Copyright (c) 2022 Jonathan Lang
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Baracuda.Monitoring.Interface;
using Baracuda.Monitoring.Internal.Profiling;
using Baracuda.Monitoring.Internal.Units;
using Baracuda.Pooling.Concretions;
using Baracuda.Reflection;
using Baracuda.Threading;
using Debug = UnityEngine.Debug;

namespace Baracuda.Monitoring.API
{
    /// <summary>
    /// Class offers public API and manages monitoring processes.
    /// </summary>
    public static class MonitoringManager
    {
        #region --- API ---

        /// <summary>
        /// Value indicated whether or not monitoring profiling has completed and monitoring is fully initialized.
        /// </summary>
        /// <exception cref="InvalidOperationException"></exception>
        public static bool IsInitialized
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => isInitialized;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private set
            {
                Dispatcher.GuardAgainstIsNotMainThread("set" + nameof(IsInitialized));
                isInitialized = value;
            }
        }
        
        /// <summary>
        /// Event is invoked when profiling process for the current system has been completed.
        /// This might even be before an Awake call which is why subscribing to this event will instantly invoke
        /// a callback when subscribing after profiling was already completed.
        /// </summary>
        public static event ProfilingCompletedListener ProfilingCompleted
        {
            add
            {
                if (IsInitialized)
                {
                    value.Invoke(staticUnitCache, instanceUnitCache);
                    return;
                }
                profilingCompleted += value;
            }
            remove => profilingCompleted -= value;
        }
        
        /// <summary>
        /// Event is called when a new <see cref="MonitorUnit"/> was created.
        /// </summary>
        public static event Action<IMonitorUnit> UnitCreated;
        
        /// <summary>
        /// Event is called when a <see cref="MonitorUnit"/> was disposed.
        /// </summary>
        public static event Action<IMonitorUnit> UnitDisposed;
        
        public delegate void ProfilingCompletedListener(IReadOnlyList<IMonitorUnit> staticUnits, IReadOnlyList<IMonitorUnit> instanceUnits);


        /*
         * Target Object Registration   
         */

        /// <summary>
        /// Register an object that is monitored during runtime.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void RegisterTarget(object target)
        {
#if !DISABLE_MONITORING
            RegisterTargetInternal(target);
#endif
        }
        
        /// <summary>
        /// Unregister an object that is monitored during runtime.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void UnregisterTarget(object target)
        {
#if !DISABLE_MONITORING
            UnregisterTargetInternal(target);
#endif
        }

        /*
         * Getter   
         */        
        
        /// <summary>
        /// Get a list of monitoring units for static targets.
        /// </summary>
        public static IReadOnlyList<IMonitorUnit> GetStaticUnits() => staticUnitCache;
        
        /// <summary>
        /// Get a list of monitoring units for instance targets.
        /// </summary>
        public static IReadOnlyList<IMonitorUnit> GetInstanceUnits() => instanceUnitCache;
        
        /// <summary>
        /// Get a list of all monitoring units.
        /// </summary>
        public static IReadOnlyList<IMonitorUnit> GetAllMonitoringUnits() => monitoringUnitCache;
        
        #endregion

        //--------------------------------------------------------------------------------------------------------------
        
        #region --- Private Fields ---
        
        private static Dictionary<Type, List<MonitorProfile>> instanceMonitorProfiles = new Dictionary<Type, List<MonitorProfile>>();
        
        private static readonly List<MonitorUnit> staticUnitCache = new List<MonitorUnit>(100);
        private static readonly List<MonitorUnit> instanceUnitCache = new List<MonitorUnit>(100);
        private static readonly List<MonitorUnit> monitoringUnitCache = new List<MonitorUnit>(200);
        
        private static readonly Dictionary<object, MonitorUnit[]> activeInstanceUnits = new Dictionary<object, MonitorUnit[]>();
        
        private static readonly List<object> registeredTargets = new List<object>(300);
        private static bool initialInstanceUnitsCreated = false;

        private static volatile bool isInitialized = false;
        private static ProfilingCompletedListener profilingCompleted;
        
                
#if DEBUG
        private static readonly HashSet<object> registeredObjects = new HashSet<object>();
#endif
        
        #endregion
        
        #region --- Raise Events ---
        
        private static void RaiseUnitCreated(MonitorUnit monitorUnit)
        {
            if (!Dispatcher.IsMainThread())
            {
                UnitCreated.Dispatch(monitorUnit);
                return;
            }
            UnitCreated?.Invoke(monitorUnit);
        }

        private static void RaiseUnitDisposed(MonitorUnit monitorUnit)
        {
            if (!Dispatcher.IsMainThread())
            {
                UnitDisposed.Dispatch(monitorUnit);
                return;
            }
            UnitDisposed?.Invoke(monitorUnit);
        }
        
        #endregion
        
        //--------------------------------------------------------------------------------------------------------------

        /*
         * Conditional Compilation   
         */

        #region --- Internal ---
        
#if !DISABLE_MONITORING
        #region --- Target Registration ---

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void RegisterTargetInternal(object target)
        {
#if DEBUG
            if (registeredObjects.Contains(target))
            {
                Debug.LogWarning($"{target} is already registered as a <b>Monitoring Target</b>!" +
                                 $"\nEnsure to not call <b>{nameof(MonitoringManager)}.{nameof(RegisterTarget)}</b> " +
                                 $"multiple times and don't make calls to " +
                                 $"<b>{nameof(MonitoringManager)}.{nameof(RegisterTarget)}</b> in classes inheriting from " +
                                 $"<b>{nameof(MonitoredBehaviour)}</b>, <b>{nameof(MonitoredObject)}</b> or similar!");
                return;
            }
            registeredObjects.Add(target);
#endif
            registeredTargets.Add(target);
            if (initialInstanceUnitsCreated)
            {
                CreateInstanceUnits(target, target.GetType());
            }
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void UnregisterTargetInternal(object target)
        {
#if DEBUG
            if (!registeredObjects.Contains(target))
            {
                Debug.LogWarning($"{target} is not registered! Ensure to not call {nameof(UnregisterTargetInternal)} multiple times!");
                return;
            }
            registeredObjects.Remove(target);
#endif
            DestroyInstanceUnits(target);
            registeredTargets.Remove(target);
        }

        #endregion
        
        #region --- Complete Profiling ---

        internal static async Task CompleteProfilingAsync(
            List<MonitorProfile> staticProfiles,
            Dictionary<Type, List<MonitorProfile>> instanceProfiles, 
            CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            
            instanceMonitorProfiles = instanceProfiles;
            
            CreateStaticUnits(staticProfiles.ToArray());
            
            await Dispatcher.InvokeAsync(CreateInitialInstanceUnits, ct);
            await Dispatcher.InvokeAsync(ProfilingCompletedInternal, ct);
        }
        
        internal static void CompleteProfiling(
            List<MonitorProfile> staticProfiles,
            Dictionary<Type, List<MonitorProfile>> instanceProfiles, 
            CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            
            instanceMonitorProfiles = instanceProfiles;
            
            CreateStaticUnits(staticProfiles.ToArray());
            CreateInitialInstanceUnits();
            ProfilingCompletedInternal();
        }
        
                
        private static void ProfilingCompletedInternal()
        {
            Dispatcher.GuardAgainstIsNotMainThread(nameof(ProfilingCompletedInternal));
            
            IsInitialized = true;
            profilingCompleted?.Invoke(staticUnitCache, instanceUnitCache);
            profilingCompleted = null;
        }
        
        #endregion

        //--------------------------------------------------------------------------------------------------------------

        #region --- Instantiate: Instance Units ---
        
        private static void CreateInitialInstanceUnits()
        {
            for (var i = 0; i < registeredTargets.Count; i++)
            {
                CreateInstanceUnits(registeredTargets[i], registeredTargets[i].GetType());
            }
            initialInstanceUnitsCreated = true;
        }

        private static void CreateInstanceUnits(object target, Type type)
        {
            var validTypes = type.GetBaseTypes(true);
            // create a new array to cache the units instances that will be created. 
            var units = ConcurrentListPool<MonitorUnit>.Get();
            var guids = ConcurrentListPool<MemberInfo>.Get();
            
            for (var i = 0; i < validTypes.Length; i++)
            {
                if(validTypes[i].IsGenericType)
                {
                    continue;
                }

                if (!instanceMonitorProfiles.TryGetValue(validTypes[i], out var profiles))
                {
                    continue;
                }

                // loop through the profiles and create a new unit for each profile.
                for (var j = 0; j < profiles.Count; j++)
                {
                    if(guids.Contains(profiles[j].MemberInfo))
                    {
                        continue;
                    }

                    guids.Add(profiles[j].MemberInfo);
                        
                    var unit = profiles[j].CreateUnit(target);
                    units.Add(unit);
                    instanceUnitCache.Add(unit);
                    monitoringUnitCache.Add(unit);
                    RaiseUnitCreated(unit);
                }
            }

            // cache the created units in a dictionary that allows access by the units target.
            // this dictionary will be used to dispose the units if the target gets destroyed 
            if (units.Count > 0 && !activeInstanceUnits.ContainsKey(target))
            {
                activeInstanceUnits.Add(target, units.ToArray());
            }
            ConcurrentListPool<MemberInfo>.Release(guids);
            ConcurrentListPool<MonitorUnit>.Release(units);
        }

        #endregion

        #region --- Dispose: Instance Units ---

        private static void DestroyInstanceUnits(object target)
        {
            if (!activeInstanceUnits.TryGetValue(target, out var units))
            {
                return;
            }

            for (var i = 0; i < units.Length; i++)
            {
                var unit = units[i];
                unit.Dispose();
                instanceUnitCache.Remove(unit);
                monitoringUnitCache.Remove(unit);
                RaiseUnitDisposed(unit);
            }
                
            activeInstanceUnits.Remove(target);
        }

        #endregion
        
        #region --- Instantiate: Static Units ---
        
        private static void CreateStaticUnits(MonitorProfile[] staticProfiles)
        {
            for (var i = 0; i < staticProfiles.Length; i++)
            {
                CreateStaticUnit(staticProfiles[i]);
            }
        }
        
        private static void CreateStaticUnit(MonitorProfile staticProfile)
        {
            var staticUnit = staticProfile.CreateUnit(null);
            staticUnitCache.Add(staticUnit);
            monitoringUnitCache.Add(staticUnit);
        }

        #endregion
        
#endif //DISABLE_MONITORING

        #endregion
    }
}
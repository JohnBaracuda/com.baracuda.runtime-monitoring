// Copyright (c) 2022 Jonathan Lang
 
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Baracuda.Monitoring.API;
using Baracuda.Monitoring.Source.Interfaces;
using Baracuda.Monitoring.Source.Profiles;
using Baracuda.Monitoring.Source.Units;
using Baracuda.Threading;
using Baracuda.Utilities.Extensions;
using Baracuda.Utilities.Pooling;
using Baracuda.Utilities.Reflection;
using UnityEngine;

namespace Baracuda.Monitoring.Source.Systems
{
    internal class MonitoringManager : IMonitoringManager, IMonitoringManagerInternal
    {
        #region --- API ---

        public bool IsInitialized
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _isInitialized;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private set
            {
                Dispatcher.GuardAgainstIsNotMainThread("set" + nameof(IsInitialized));
                _isInitialized = value;
            }
        }

        /*
         * Events   
         */

        public event ProfilingCompletedListener ProfilingCompleted
        {
            add
            {
                if (IsInitialized)
                {
                    value.Invoke(_staticUnitCache, _instanceUnitCache);
                    return;
                }
                _profilingCompleted += value;
            }
            remove => _profilingCompleted -= value;
        }

        public event Action<IMonitorUnit> UnitCreated;

        public event Action<IMonitorUnit> UnitDisposed;
        
        /*
         * Target Object Registration   
         */

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RegisterTarget<T>(T target) where T : class
        {
            RegisterTargetInternal(target);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void UnregisterTarget<T>(T target) where T : class
        {
            UnregisterTargetInternal(target);
        }

        /*
         * Getter   
         */        
        
        [Pure] 
        public IReadOnlyList<IMonitorUnit> GetStaticUnits() => _staticUnitCache;
        
        [Pure]
        public IReadOnlyList<IMonitorUnit> GetInstanceUnits() => _instanceUnitCache;
        
        [Pure] 
        public IReadOnlyList<IMonitorUnit> GetAllMonitoringUnits() => _monitoringUnitCache;
        

        #endregion

        //--------------------------------------------------------------------------------------------------------------
        
        #region --- Private Fields ---
        
        private Dictionary<Type, List<MonitorProfile>> _instanceMonitorProfiles = new Dictionary<Type, List<MonitorProfile>>();
        
        private readonly List<MonitorUnit> _staticUnitCache = new List<MonitorUnit>(100);
        private readonly List<MonitorUnit> _instanceUnitCache = new List<MonitorUnit>(100);
        private readonly List<MonitorUnit> _monitoringUnitCache = new List<MonitorUnit>(200);
        
        private readonly Dictionary<object, MonitorUnit[]> _activeInstanceUnits = new Dictionary<object, MonitorUnit[]>();
        
        private readonly List<object> _registeredTargets = new List<object>(300);
        private bool _initialInstanceUnitsCreated = false;

        private volatile bool _isInitialized = false;
        private ProfilingCompletedListener _profilingCompleted;
        
#if DEBUG
        private readonly HashSet<object> _registeredObjects = new HashSet<object>();
#endif
        
        #endregion
        
        #region --- Raise Events ---
        
        private void RaiseUnitCreated(IMonitorUnit monitorUnit)
        {
            if (!Dispatcher.IsMainThread())
            {
                UnitCreated.Dispatch(monitorUnit);
                return;
            }
            UnitCreated?.Invoke(monitorUnit);
        }

        private void RaiseUnitDisposed(IMonitorUnit monitorUnit)
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
 
        #region --- Target Registration ---

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void RegisterTargetInternal<T>(T target) where T : class
        {
#if DEBUG
            if (_registeredObjects.Contains(target))
            {
                Debug.LogWarning($"{target} is already registered as a <b>Monitoring Target</b>!" +
                                 $"\nEnsure to not call <b>{nameof(IMonitoringManager)}.{nameof(RegisterTarget)}</b> " +
                                 $"multiple times and don't make calls to " +
                                 $"<b>{nameof(IMonitoringManager)}.{nameof(RegisterTarget)}</b> in classes inheriting from " +
                                 $"<b>{nameof(MonitoredBehaviour)}</b>, <b>{nameof(MonitoredObject)}</b> or similar!");
                return;
            }
            _registeredObjects.Add(target);
#endif
            _registeredTargets.Add(target);
            if (_initialInstanceUnitsCreated)
            {
                CreateInstanceUnits(target, target.GetType());
            }
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void UnregisterTargetInternal<T>(T target) where T : class
        {
#if DEBUG
            if (!_registeredObjects.Contains(target))
            {
                Debug.LogWarning($"{target} is not registered! Ensure to not call {nameof(UnregisterTargetInternal)} multiple times!");
                return;
            }
            _registeredObjects.Remove(target);
#endif
            DestroyInstanceUnits(target);
            _registeredTargets.Remove(target);
        }

        #endregion
        
        #region --- Complete Profiling ---

        public async Task CompleteProfilingAsync(
            List<MonitorProfile> staticProfiles,
            Dictionary<Type, List<MonitorProfile>> instanceProfiles, 
            CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            
            _instanceMonitorProfiles = instanceProfiles;
            
            CreateStaticUnits(staticProfiles.ToArray());

            if (Dispatcher.IsMainThread())
            {
                CreateInitialInstanceUnits();
                ProfilingCompletedInternal();
            }
            else
            {
                await Dispatcher.InvokeAsync(CreateInitialInstanceUnits, ct);
                await Dispatcher.InvokeAsync(ProfilingCompletedInternal, ct);
            }
        }
       
                
        private void ProfilingCompletedInternal()
        {
            Dispatcher.GuardAgainstIsNotMainThread(nameof(ProfilingCompletedInternal));
            
            IsInitialized = true;
            _profilingCompleted?.Invoke(_staticUnitCache, _instanceUnitCache);
            _profilingCompleted = null;
        }
        
        #endregion

        //--------------------------------------------------------------------------------------------------------------

        #region --- Instantiate: Instance Units ---
        
        private void CreateInitialInstanceUnits()
        {
            for (var i = 0; i < _registeredTargets.Count; i++)
            {
                CreateInstanceUnits(_registeredTargets[i], _registeredTargets[i].GetType());
            }
            _initialInstanceUnitsCreated = true;
        }

        private void CreateInstanceUnits(object target, Type type)
        {
            var validTypes = type.GetBaseTypes(true, true);
            // create a new array to cache the units instances that will be created. 
            var units = ConcurrentListPool<MonitorUnit>.Get();
            var guids = ConcurrentListPool<MemberInfo>.Get();
            
            for (var i = 0; i < validTypes.Length; i++)
            {
                if(validTypes[i].IsGenericType)
                {
                    continue;
                }

                if (!_instanceMonitorProfiles.TryGetValue(validTypes[i], out var profiles))
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
                    _instanceUnitCache.Add(unit);
                    _monitoringUnitCache.Add(unit);
                    RaiseUnitCreated(unit);
                }
            }

            // cache the created units in a dictionary that allows access by the units target.
            // this dictionary will be used to dispose the units if the target gets destroyed 
            if (units.Count > 0 && !_activeInstanceUnits.ContainsKey(target))
            {
                _activeInstanceUnits.Add(target, units.ToArray());
            }
            ConcurrentListPool<MemberInfo>.Release(guids);
            ConcurrentListPool<MonitorUnit>.Release(units);
        }

        #endregion

        #region --- Dispose: Instance Units ---

        private void DestroyInstanceUnits(object target)
        {
            if (!_activeInstanceUnits.TryGetValue(target, out var units))
            {
                return;
            }

            for (var i = 0; i < units.Length; i++)
            {
                var unit = units[i];
                unit.Dispose();
                _instanceUnitCache.Remove(unit);
                _monitoringUnitCache.Remove(unit);
                RaiseUnitDisposed(unit);
            }
                
            _activeInstanceUnits.Remove(target);
        }

        #endregion
        
        #region --- Instantiate: Static Units ---
        
        private void CreateStaticUnits(MonitorProfile[] staticProfiles)
        {
            for (var i = 0; i < staticProfiles.Length; i++)
            {
                CreateStaticUnit(staticProfiles[i]);
            }
        }
        
        private void CreateStaticUnit(MonitorProfile staticProfile)
        {
            var staticUnit = staticProfile.CreateUnit(null);
            _staticUnitCache.Add(staticUnit);
            _monitoringUnitCache.Add(staticUnit);
        }

        #endregion
    }
}
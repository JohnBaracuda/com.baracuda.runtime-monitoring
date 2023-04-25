using Baracuda.Monitoring.Profiles;
using Baracuda.Monitoring.Types;
using Baracuda.Monitoring.Units;
using Baracuda.Monitoring.Utilities.Extensions;
using Baracuda.Monitoring.Utilities.Pooling;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Reflection;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Baracuda.Monitoring
{
#pragma warning disable CS0067

    internal class MonitoringRegistry : IMonitoringRegistry
    {
        #region Data

        private Dictionary<Type, List<MonitorProfile>> _instanceMonitorProfiles;
        private List<MonitorProfile> _staticProfiles;

        private readonly List<string> _usedTags = new List<string>();
        private readonly List<string> _usedFonts = new List<string>();
        private readonly List<string> _usedTypeNames = new List<string>();
        private readonly List<Type> _usedTypes = new List<Type>();

        private readonly List<MonitorHandle> _staticMonitorHandles = new List<MonitorHandle>(128);
        private readonly List<MonitorHandle> _instanceMonitorHandles = new List<MonitorHandle>(128);
        private readonly List<MonitorHandle> _monitorHandles = new List<MonitorHandle>(128);

        private readonly Dictionary<object, MonitorHandle[]> _activeInstanceHandles =
            new Dictionary<object, MonitorHandle[]>();
        private readonly List<object> _registeredTargets = new List<object>(256);

        #endregion


        #region Public

        /// <summary>
        ///     Get a list of monitoring handles for all targets.
        /// </summary>
        [Pure]
        public IReadOnlyList<IMonitorHandle> GetMonitorHandles(HandleTypes handleTypes = HandleTypes.All)
        {
            switch (handleTypes)
            {
                case HandleTypes.None:
                    return null;
                case HandleTypes.Instance:
                    return _instanceMonitorHandles;
                case HandleTypes.Static:
                    return _staticMonitorHandles;
                case HandleTypes.All:
                    return _monitorHandles;
                default:
                    throw new ArgumentOutOfRangeException(nameof(handleTypes), handleTypes, null);
            }
        }

        /// <summary>
        ///     Get a list of <see cref="IMonitorHandle" />s registered to the passed target object.
        /// </summary>
        [Pure]
        public IMonitorHandle[] GetMonitorHandlesForTarget<T>(T target) where T : class
        {
            if (!Monitor.Initialized)
            {
                Debug.LogWarning(
                    $"Calling {nameof(GetMonitorHandlesForTarget)} before profiling has completed. " +
                    "If you need to access units during initialization consider disabling async profiling in the monitoring settings!");
            }

            var list = ListPool<IMonitorHandle>.Get();
            var monitorUnits = GetMonitorHandles(HandleTypes.Instance);
            for (var i = 0; i < monitorUnits.Count; i++)
            {
                var instanceUnit = monitorUnits[i];
                if (instanceUnit.Target == target)
                {
                    list.Add(instanceUnit);
                }
            }

            var returnValue = list.ToArray();
            ListPool<IMonitorHandle>.Release(list);
            return returnValue;
        }

        public IReadOnlyList<string> UsedTags => _usedTags;
        public IReadOnlyList<string> UsedFonts => _usedFonts;
        public IReadOnlyList<Type> UsedTypes => _usedTypes;
        public IReadOnlyList<string> UsedTypeNames => _usedTypeNames;

        #endregion


        #region Internal

        internal void AddUsedTag(string tag)
        {
            _usedTags.AddUnique(tag);
        }

        internal void AddUsedFont(string font)
        {
            _usedFonts.AddUnique(font);
        }

        internal void AddUsedType(Type type)
        {
            _usedTypes.AddUnique(type);
            _usedTypeNames.AddUnique(type.HumanizedName());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void RegisterTargetInternal<T>(T target) where T : class
        {
            if (!_registeredTargets.AddUnique(target))
            {
                return;
            }

            if (Monitor.Initialized)
            {
                CreateInstanceMonitorHandles(target, target.GetType());
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void UnregisterTargetInternal<T>(T target) where T : class
        {
            DestroyMonitorHandleForTarget(target);
            _registeredTargets.Remove(target);
        }

        internal void RegisterProfiles(Dictionary<Type, List<MonitorProfile>> instanceProfiles,
            List<MonitorProfile> staticProfiles)
        {
            _instanceMonitorProfiles = instanceProfiles;
            _staticProfiles = staticProfiles;
            CreateInitialInstanceMonitorHandles();
            CreateStaticMonitorHandles();
            Monitor.InternalEvents.RaiseProfilingCompleted(_staticMonitorHandles, _instanceMonitorHandles);
        }

        #endregion


        #region Instance Monitor Handles

        private void CreateInitialInstanceMonitorHandles()
        {
            for (var i = 0; i < _registeredTargets.Count; i++)
            {
                CreateInstanceMonitorHandles(_registeredTargets[i], _registeredTargets[i].GetType());
            }
        }

        private void CreateInstanceMonitorHandles(object target, Type type)
        {
            var validTypes = type.GetBaseTypes(true, true);

            // create a new array to cache the units instances that will be created.
            var units = ListPool<MonitorHandle>.Get();
            var guids = ListPool<MemberInfo>.Get();

            for (var i = 0; i < validTypes.Length; i++)
            {
                if (validTypes[i].IsGenericType)
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
                    if (guids.Contains(profiles[j].MemberInfo))
                    {
                        continue;
                    }

                    guids.Add(profiles[j].MemberInfo);
                    var unit = profiles[j].CreateUnit(target);
                    units.Add(unit);
                    _instanceMonitorHandles.Add(unit);
                    _monitorHandles.Add(unit);
                    Monitor.InternalEvents.RaiseMonitorHandleCreated(unit);
                }
            }

            // cache the created units in a dictionary that allows access by the units target.
            // this dictionary will be used to dispose the units if the target gets destroyed
            if (units.Count > 0 && !_activeInstanceHandles.ContainsKey(target))
            {
                _activeInstanceHandles.Add(target, units.ToArray());
            }

            ListPool<MemberInfo>.Release(guids);
            ListPool<MonitorHandle>.Release(units);
        }

        private void DestroyMonitorHandleForTarget(object target)
        {
            if (!_activeInstanceHandles.TryGetValue(target, out var units))
            {
                return;
            }

            for (var i = 0; i < units.Length; i++)
            {
                var unit = units[i];
                unit.Dispose();
                _instanceMonitorHandles.Remove(unit);
                _monitorHandles.Remove(unit);
                Monitor.InternalEvents.RaiseMonitorHandleDisposed(unit);
            }

            _activeInstanceHandles.Remove(target);
        }

        #endregion


        #region Static Monitor Handles

        private void CreateStaticMonitorHandles()
        {
            for (var i = 0; i < _staticProfiles.Count; i++)
            {
                CreateStaticMonitorHandle(_staticProfiles[i]);
            }
        }

        private void CreateStaticMonitorHandle(MonitorProfile staticProfile)
        {
            var staticUnit = staticProfile.CreateUnit(null);
            _staticMonitorHandles.Add(staticUnit);
            _monitorHandles.Add(staticUnit);
        }

        #endregion


        #region Ctor

        internal MonitoringRegistry(MonitoringRegistry old)
        {
            if (old == null)
            {
                return;
            }

            foreach (var target in old._registeredTargets)
            {
                _registeredTargets.AddUnique(target);
            }
        }

        #endregion
    }
}
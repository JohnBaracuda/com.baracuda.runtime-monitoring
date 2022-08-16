// Copyright (c) 2022 Jonathan Lang

using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Baracuda.Monitoring.API;
using Baracuda.Monitoring.Source.Interfaces;
using Baracuda.Utilities.Pooling;
using JetBrains.Annotations;
using UnityEngine;

namespace Baracuda.Monitoring.Source.Systems
{
    internal class MonitoringUtility : IMonitoringUtility, IMonitoringUtilityInternal
    {
        private readonly IMonitoringManager _monitoringManager;
        private readonly HashSet<string> _tags = new HashSet<string>();
        private readonly HashSet<string> _typeStrings = new HashSet<string>();

        internal MonitoringUtility(IMonitoringManager monitoringManager)
        {
            _monitoringManager = monitoringManager;
        }
        
        //--------------------------------------------------------------------------------------------------------------

        private readonly HashSet<int> _fontHashSet = new HashSet<int>();
        
        public bool IsFontHashUsed(int fontHash)
        {
            return _fontHashSet.Contains(fontHash);
        }
        
        public void AddFontHash(int fontHash)
        {
            _fontHashSet.Add(fontHash);
        }

        public void AddTag(string tag)
        {
            _tags.Add(tag);
        }

        public void AddTypeString(string typeString)
        {
            _typeStrings.Add(typeString);
        }

        //--------------------------------------------------------------------------------------------------------------
        
        [Pure]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IMonitorUnit[] GetMonitorUnitsForTarget(object target)
        {
            if (!_monitoringManager.IsInitialized)
            {
                Debug.LogWarning(
                    $"Calling {nameof(GetMonitorUnitsForTarget)} before profiling has completed. " +
                    $"If you need to access units during initialization consider disabling async profiling in the monitoring settings!");
            }

            var list = ListPool<IMonitorUnit>.Get();
            var monitorUnits = _monitoringManager.GetInstanceUnits();
            for (var i = 0; i <monitorUnits.Count; i++)
            {
                var instanceUnit = monitorUnits[i];
                if (instanceUnit.Target == target)
                {
                    list.Add(instanceUnit);
                }
            }
            var returnValue = list.ToArray();
            ListPool<IMonitorUnit>.Release(list);
            return returnValue;
        }

        public IReadOnlyCollection<string> GetAllTags()
        {
            return _tags;
        }
        
        public IReadOnlyCollection<string> GetAllTypeStrings()
        {
            return _typeStrings;
        }
    }
}
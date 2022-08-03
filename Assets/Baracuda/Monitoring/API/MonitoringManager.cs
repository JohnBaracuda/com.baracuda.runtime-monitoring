// Copyright (c) 2022 Jonathan Lang

using System;
using System.Collections.Generic;
using Baracuda.Monitoring.Source.Interfaces;
using JetBrains.Annotations;

namespace Baracuda.Monitoring.API
{
    /// <summary>
    /// Class offers public API and manages monitoring processes.
    /// </summary>
    [Obsolete("Use IMonitoringManager instead. Resolve registered instance using MonitoringSystems.Resolve<IMonitoringManager>()")]
    public static class MonitoringManager
    {
        [Obsolete("Use IMonitoringManager instead. Resolve registered instance using MonitoringSystems.Resolve<IMonitoringManager>()")]
        public static bool IsInitialized => MonitoringSystems.Resolve<IMonitoringManager>().IsInitialized;

        [Obsolete("Use IMonitoringManager instead. Resolve registered instance using MonitoringSystems.Resolve<IMonitoringManager>()")]
        public static event ProfilingCompletedListener ProfilingCompleted
        {
            add => MonitoringSystems.Resolve<IMonitoringManager>().ProfilingCompleted += value;
            remove => MonitoringSystems.Resolve<IMonitoringManager>().ProfilingCompleted -= value;
        }
        
        [Obsolete("Use IMonitoringManager instead. Resolve registered instance using MonitoringSystems.Resolve<IMonitoringManager>()")]
        public static event Action<IMonitorUnit> UnitCreated
        {
            add => MonitoringSystems.Resolve<IMonitoringManager>().UnitCreated += value;
            remove => MonitoringSystems.Resolve<IMonitoringManager>().UnitCreated -= value;
        }
        
        [Obsolete("Use IMonitoringManager instead. Resolve registered instance using MonitoringSystems.Resolve<IMonitoringManager>()")]
        public static event Action<IMonitorUnit> UnitDisposed
        {
            add => MonitoringSystems.Resolve<IMonitoringManager>().UnitDisposed += value;
            remove => MonitoringSystems.Resolve<IMonitoringManager>().UnitDisposed -= value;
        }

        /*
         * Target Object Registration   
         */

        [Obsolete("Use IMonitoringManager instead. Resolve registered instance using MonitoringSystems.Resolve<IMonitoringManager>()")]
        public static void RegisterTarget(object target)
        {
            MonitoringSystems.Resolve<IMonitoringManager>().RegisterTarget(target);
        }
        
        /// <summary>
        /// Unregister an object that is monitored during runtime.
        /// </summary>
        [Obsolete("Use IMonitoringManager instead. Resolve registered instance using MonitoringSystems.Resolve<IMonitoringManager>()")]
        public static void UnregisterTarget(object target)
        {
            MonitoringSystems.Resolve<IMonitoringManager>().UnregisterTarget(target);
        }

        /*
         * Unit for target   
         */

        /// <summary>
        /// Get a collection of <see cref="IMonitorUnit"/>s associated with the passed target. 
        /// </summary>
        [Pure]
        [Obsolete("Use IMonitoringUtility instead. Resolve registered instance using MonitoringSystems.Resolve<IMonitoringUtility>()")]
        public static IMonitorUnit[] GetMonitorUnitsForTarget(object target)
        {
            return MonitoringSystems.Resolve<IMonitoringUtility>().GetMonitorUnitsForTarget(target);
        }

        /*
         * Getter   
         */        
        
        [Pure]
        [Obsolete("Use IMonitoringManager instead. Resolve registered instance using MonitoringSystems.Resolve<IMonitoringManager>()")]
        public static IReadOnlyList<IMonitorUnit> GetStaticUnits()
        { 
            return MonitoringSystems.Resolve<IMonitoringManager>().GetStaticUnits();
        }
        
        [Pure]
        [Obsolete("Use IMonitoringManager instead. Resolve registered instance using MonitoringSystems.Resolve<IMonitoringManager>()")]
        public static IReadOnlyList<IMonitorUnit> GetInstanceUnits()
        { 
            return MonitoringSystems.Resolve<IMonitoringManager>().GetInstanceUnits();
        }
        
        [Pure] 
        [Obsolete("Use IMonitoringManager instead. Resolve registered instance using MonitoringSystems.Resolve<IMonitoringManager>()")]
        public static IReadOnlyList<IMonitorUnit> GetAllMonitoringUnits()
        { 
            return MonitoringSystems.Resolve<IMonitoringManager>().GetAllMonitoringUnits();
        }

        /*
         * Optimization Misc   
         */

        [Obsolete("Use IMonitoringManager instead. Resolve registered instance using MonitoringSystems.Resolve<IMonitoringManager>()")]
        public static bool IsFontHasUsed(int fontHash)
        {
            return MonitoringSystems.Resolve<IMonitoringUtility>().IsFontHashUsed(fontHash);
        }

        [Obsolete("Use IMonitoringManager instead. Resolve registered instance using MonitoringSystems.Resolve<IMonitoringManager>()")]
        public static bool ValidationTickEnabled
        {
            get => MonitoringSystems.Resolve<IMonitoringTicker>().ValidationTickEnabled;
            set => MonitoringSystems.Resolve<IMonitoringTicker>().ValidationTickEnabled = value;
        }
    }
}
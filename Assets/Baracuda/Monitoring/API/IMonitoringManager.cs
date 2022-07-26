// Copyright (c) 2022 Jonathan Lang
 
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Baracuda.Monitoring.Source.Units;
using JetBrains.Annotations;

namespace Baracuda.Monitoring.API
{
    public interface IMonitoringManager : IMonitoringSystem<IMonitoringManager>
    {
        /// <summary>
        /// Value indicated whether or not monitoring profiling has completed and monitoring is fully initialized.
        /// </summary>
        /// <exception cref="InvalidOperationException"></exception>
        bool IsInitialized { get; }

        /// <summary>
        /// Event is invoked when profiling process for the current system has been completed.
        /// This might even be before an Awake call which is why subscribing to this event will instantly invoke
        /// a callback when subscribing after profiling was already completed.
        /// </summary>
        event ProfilingCompletedListener ProfilingCompleted;
        
        /// <summary>
        /// Event is called when a new <see cref="MonitorUnit"/> was created.
        /// </summary>
        event Action<IMonitorUnit> UnitCreated;
        
        /// <summary>
        /// Event is called when a <see cref="MonitorUnit"/> was disposed.
        /// </summary>
        event Action<IMonitorUnit> UnitDisposed;
        
        /*
         * Target Object Registration   
         */

        /// <summary>
        /// Register an object that is monitored during runtime.
        /// </summary>
        void RegisterTarget<T>(T target) where T : class;
        
        /// <summary>
        /// Unregister an object that is monitored during runtime.
        /// </summary>
        void UnregisterTarget<T>(T target) where T : class;

        /*
         * Unit for target   
         */

        /// <summary>
        /// Get a collection of <see cref="IMonitorUnit"/>s associated with the passed target. 
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [Pure] IMonitorUnit[] GetMonitorUnitsForTarget(object target);

        /*
         * Getter   
         */        
        
        /// <summary>
        /// Get a list of monitoring units for static targets.
        /// </summary>
        [Pure] IReadOnlyList<IMonitorUnit> GetStaticUnits();
        
        /// <summary>
        /// Get a list of monitoring units for instance targets.
        /// </summary>
        [Pure] IReadOnlyList<IMonitorUnit> GetInstanceUnits();
        
        /// <summary>
        /// Get a list of all monitoring units.
        /// </summary>
        [Pure] IReadOnlyList<IMonitorUnit> GetAllMonitoringUnits();
        
        /// <summary>
        /// Method returns true of the passed hash from the name of a font asset is used by a MFontAttribute and therefore
        /// required by a monitoring unit. Used to dynamically load/unload required fonts.
        /// </summary>
        /// <param name="fontHash">The hash of the fonts name (string)</param>
        [Pure] bool IsFontHasUsed(int fontHash);
    }
}
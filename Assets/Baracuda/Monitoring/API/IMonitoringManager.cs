// Copyright (c) 2022 Jonathan Lang

using System;
using System.Collections.Generic;
using Baracuda.Monitoring.Source.Units;
using JetBrains.Annotations;

namespace Baracuda.Monitoring.API
{
    /// <summary>
    /// Core interface for accessing Runtime Monitoring functionality.
    /// </summary>
    public interface IMonitoringManager : IMonitoringSubsystem<IMonitoringManager>
    {
        /// <summary>
        /// Value indicated whether or not monitoring profiling has completed and monitoring is fully initialized.
        /// </summary>
        /// <exception cref="InvalidOperationException"></exception>
        bool IsInitialized { get; }

        /// <summary>
        /// Event is invoked when profiling process for the current system has been completed.
        /// Subscribing to this event will instantly invoke a callback if profiling has already completed.
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
    }
}
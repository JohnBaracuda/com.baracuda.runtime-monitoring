// Copyright (c) 2022 Jonathan Lang
using System;
using Baracuda.Monitoring.Internal.Units;

namespace Baracuda.Monitoring.Interface
{
    public interface IMonitorUnit
    {
        /// <summary>
        /// Readable member name.
        /// </summary>
        string Name { get; }
        
        /// <summary>
        /// The target object of the monitored member. Null if static
        /// </summary>
        object Target { get; }

        /// <summary>
        /// <see cref="IMonitorProfile"/> describing the monitored member. 
        /// </summary>
        IMonitorProfile Profile { get; }

        /// <summary>
        /// Force the monitored member to update its state. This will invoke a <see cref="MonitorUnit.ValueUpdated"/> event.
        /// </summary>
        void Refresh();

        /// <summary>
        /// The active state of the unit. Only active units are evaluated and updated. 
        /// </summary>
        bool Enabled { get; set; }
        
        /// <summary>
        /// Event is invoked when the units active state has changed.
        /// </summary>
        event Action<bool> ActiveStateChanged;

        /// <summary>
        /// Event is invoked when the value of the monitored member has changed.
        /// </summary>
        event Action<string> ValueUpdated;

        /// <summary>
        /// Event is invoked when the unit is being disposed.
        /// </summary>
        event Action Disposing;
        
        /// <summary>
        /// Unique Id
        /// </summary>
        int UniqueID { get; }

        /// <summary>
        /// Get the current value or state of the monitored member as a formatted string. 
        /// </summary>
        string GetState();
        
        
        //--------------------------------------------------------------------------------------------------------------
        
        #region --- Obsolete ---
        
        [Obsolete("use GetState instead!")]
        string GetStateFormatted { get; }
        
        [Obsolete("use GetValue for IValueUnit instead!")]
        string GetStateRaw { get; }
        
        #endregion
    }
}
// Copyright (c) 2022 Jonathan Lang
using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Baracuda.Monitoring.Interface;
using Baracuda.Monitoring.Internal.Profiling;
using UnityEngine;

namespace Baracuda.Monitoring.Internal.Units
{
    /// <summary>
    /// Object wrapping and handling the monitoring of a monitored member.
    /// </summary>
    public abstract class MonitorUnit : IDisposable, IMonitorUnit
    {
        #region --- Delegates ---

        protected delegate string StringDelegate();

        #endregion
        
        #region --- Properties ---

        /// <summary>
        /// 
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Get the current value or state of the monitored member as a formatted string. 
        /// </summary>
        public abstract string GetState();

        /// <summary>
        /// The target object of the monitored member. Null if static
        /// </summary>
        public object Target { get; }
        
        /// <summary>
        /// The <see cref="MonitorProfile"/> of the monitored member.
        /// </summary>
        public abstract IMonitorProfile Profile { get; }
        
        /// <summary>
        /// Unique UniqueID
        /// </summary>
        public int UniqueID { get; }
        
        /// <summary>
        /// The active state of the unit. Only active units are updated / evaluated.
        /// </summary>
        public bool Enabled
        {
            get => _isActive;
            set
            {
                if (_isActive == value)
                {
                    return;
                }

                _isActive = value;
                ActiveStateChanged?.Invoke(_isActive);
            }
        }

        #endregion

        #region --- Obsolete ---
        
        public string GetStateFormatted => GetState();
        public string GetStateRaw => (this as IValueUnit)?.GetValueAsObject().ToString();

        #endregion

        #region --- Fields ---

        protected const string NULL = "<color=red>NULL</color>";
        private static int backingID;
        private bool _isActive = true;

        #endregion
        
        #region --- Unit State ---

        /// <summary>
        /// Force the unit to update its state. This will invoke a <see cref="ValueUpdated"/> event.
        /// </summary>
        public abstract void Refresh();

        #endregion
        
        //--------------------------------------------------------------------------------------------------------------
        
        #region --- Events ---

        /// <summary>
        /// Event is invoked when the value of the unit has changed.
        /// </summary>
        public event Action<string> ValueUpdated;
        
        /// <summary>
        /// Event is invoked when the unit is being disposed.
        /// </summary>
        public event Action Disposing;

        /// <summary>
        /// Event is invoked when the units active state has changed.
        /// </summary>
        public event Action<bool> ActiveStateChanged; 

        #endregion
        
        //--------------------------------------------------------------------------------------------------------------

        #region --- Raise ---

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void RaiseValueChanged(string value)
        {
            ValueUpdated?.Invoke(value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void RaiseDisposing()
        {
            Disposing?.Invoke();
        }

        #endregion

        //--------------------------------------------------------------------------------------------------------------
        
        #region --- Ctor ---

        protected MonitorUnit(object target, MonitorProfile profile)
        {
            Target = target;
            Name = (target is UnityEngine.Object unityObject)
                ? unityObject.name
                : profile.UnitTargetType.Name;
            UniqueID = backingID++;
        }

        #endregion
        
        //--------------------------------------------------------------------------------------------------------------
        
        #region --- Overrides & Interfaces ---

        public virtual void Dispose()
        {
            RaiseDisposing();
        }

        #endregion
    }
}
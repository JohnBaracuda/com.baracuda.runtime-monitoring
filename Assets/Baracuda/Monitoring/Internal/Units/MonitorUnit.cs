// Copyright (c) 2022 Jonathan Lang (CC BY-NC-SA 4.0)
using System;
using System.Runtime.CompilerServices;
using Baracuda.Monitoring.Interface;
using Baracuda.Monitoring.Internal.Profiling;
using Baracuda.Pooling.Concretions;

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

        public string Name { get; }
        
        /// <summary>
        /// Get the current value or state of the monitored member as a formatted string. 
        /// </summary>
        public abstract string GetStateFormatted { get; }
        
        
        /// <summary>
        /// Get the current value or state of the monitored member as a string.
        /// </summary>
        public abstract string GetStateRaw { get; }


        /// <summary>
        /// The target object of the monitored member. Null if static
        /// </summary>
        public object Target { get; }
      
        
        /// <summary>
        /// Determines if the monitored member must be updated/refreshed from an external source.
        /// </summary>
        public bool ExternalUpdateRequired { get; protected set; } = true;

        
        /// <summary>
        /// The <see cref="MonitorProfile"/> of the monitored member.
        /// </summary>
        public abstract IMonitorProfile Profile { get; }
        
        #endregion

        #region --- Fields ---

        protected const string NULL = "<color=red>NULL</color>";

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
        }

        #endregion
        
        //--------------------------------------------------------------------------------------------------------------
        
        #region --- Overrides & Interfaces ---

        public virtual void Dispose()
        {
            RaiseDisposing();
        }
        
        public override string ToString()
        {
            var sb = StringBuilderPool.Get();
            sb.Append("Label: ");
            sb.Append(Profile.FormatData.Label);
            sb.Append(" :: Target:");
            sb.Append(Target?.ToString() ?? NULL);
            sb.Append(" :: Update:");
            sb.Append(ExternalUpdateRequired.ToString());
            return StringBuilderPool.Release(sb);
        }

        #endregion
    }
}
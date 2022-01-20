using System;
using System.Runtime.CompilerServices;
using Baracuda.Monitoring.Interface;
using Baracuda.Monitoring.Internal.Profiling;
using Baracuda.Pooling.Concretions;

namespace Baracuda.Monitoring.Internal.Units
{
    public abstract class MonitorUnit : IDisposable, IMonitorUnit
    {
        #region --- [DELEGATES] ---

        protected delegate string StringDelegate();

        #endregion
        
        #region --- [PROPERTIES] ---

        public string Name { get; }
        
        /// <summary>
        /// Get the current value of the unit as a formatted string. 
        /// </summary>
        public abstract string GetValueFormatted { get; }
        
        
        /// <summary>
        /// Get the current value of the unit as a string.
        /// </summary>
        public abstract string GetValueRaw { get; }


        /// <summary>
        /// The target object of the unit. Null if static
        /// </summary>
        public object Target { get; }
      
        
        /// <summary>
        /// Determines if the unit must be updated/refreshed from an external source.
        /// </summary>
        public bool ExternalUpdateRequired { get; protected set; } = true;

        
        /// <summary>
        /// The <see cref="MonitorProfile"/> of the unit.
        /// </summary>
        public abstract IMonitorProfile Profile { get; }
        
        #endregion

        #region --- [FIELDS] ---

        protected const string NULL = "<color=red>NULL</color>";

        #endregion
        
        #region --- [UNIT STATE] ---

        /// <summary>
        /// Force the unit to update its state. This will invoke a <see cref="ValueUpdated"/> event.
        /// </summary>
        public abstract void Refresh();
        
        #endregion
        
        //--------------------------------------------------------------------------------------------------------------
        
        #region --- [EVENTS] ---

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

        #region --- [RAISE] ---

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
        
        #region --- [CTOR] ---

        protected MonitorUnit(object target, MonitorProfile profile)
        {
            Target = target;
            Name = (target is UnityEngine.Object unityObject)
                ? unityObject.name
                : profile.UnitDeclaringType.Name;
        }

        #endregion
        
        //--------------------------------------------------------------------------------------------------------------
        
        #region --- [OVERRIDES & INTERFACES] ---

        public virtual void Dispose()
        {
            RaiseDisposing();
        }
        
        public override string ToString()
        {
            var sb = StringBuilderPool.Get();
            sb.Append("Label: ");
            sb.Append(Profile.Label);
            sb.Append(" :: Target:");
            sb.Append(Target?.ToString() ?? NULL);
            sb.Append(" :: Update:");
            sb.Append(ExternalUpdateRequired.ToString());
            return StringBuilderPool.Release(sb);
        }

        #endregion
    }
}
// Copyright (c) 2022 Jonathan Lang

using Baracuda.Monitoring.Profiles;
using System;
using System.Runtime.CompilerServices;
using Object = UnityEngine.Object;

namespace Baracuda.Monitoring.Units
{
    /// <summary>
    ///     Object wrapping and handling the monitoring of a monitored member.
    /// </summary>
    internal abstract class MonitorHandle : IDisposable, IMonitorHandle,
#pragma warning disable CS0618
        IMonitorUnit
#pragma warning restore CS0618
    {
        #region Delegates

        protected delegate string StringDelegate();

        #endregion


        #region Properties

        /// <summary>
        ///     Name of the monitored member.
        /// </summary>
        public string Name { get; }

        /// <summary>
        ///     Readable target object display name.
        /// </summary>
        public string DisplayName { get; }

        /// <summary>
        ///     Get the current value or state of the monitored member as a formatted string.
        /// </summary>
        public abstract string GetState();

        /// <summary>
        /// Get the current value or state as an unmodified string.
        /// </summary>
        public abstract object GetValueAsObject();

        /// <summary>
        ///     The target object of the monitored member. Null if static
        /// </summary>
        public object Target { get; }

        /// <summary>
        ///     The <see cref="MonitorProfile" /> of the monitored member.
        /// </summary>
        public IMonitorProfile Profile { get; }

        /// <summary>
        ///     Unique UniqueID
        /// </summary>
        public int UniqueID { get; }

        /// <summary>
        ///     The active state of the unit. Only active units are updated / evaluated.
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
                if (Profile.ReceiveTick)
                {
                    if (value)
                    {
                        Monitor.MonitoringUpdateEvents.AddUpdateTicker(this);
                    }
                    else
                    {
                        Monitor.MonitoringUpdateEvents.RemoveUpdateTicker(this);
                    }
                }

                _isActive = value;
                ActiveStateChanged?.Invoke(_isActive);
            }
        }

        #endregion


        #region Fields

        protected const string Null = "<color=red>NULL</color>";
        private static int backingID;
        private bool _isActive;

        #endregion


        #region Unit State

        /// <summary>
        ///     Force the unit to update its state. This will invoke a <see cref="ValueUpdated" /> event.
        /// </summary>
        public abstract void Refresh();

        #endregion


        //--------------------------------------------------------------------------------------------------------------


        #region Events

        /// <summary>
        ///     Event is invoked when the value of the unit has changed.
        /// </summary>
        public event Action<string> ValueUpdated;

        /// <summary>
        ///     Event is invoked when the unit is being disposed.
        /// </summary>
        public event Action Disposing;

        /// <summary>
        ///     Event is invoked when the units active state has changed.
        /// </summary>
        public event Action<bool> ActiveStateChanged;

        #endregion


        //--------------------------------------------------------------------------------------------------------------


        #region Raise

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


        #region Ctor

        protected MonitorHandle(object target, IMonitorProfile profile)
        {
            Profile = profile;
            Target = target;
            if (target is Object unityObject)
            {
                DisplayName = profile.DeclaringType.IsInterface
                    ? $"{target.GetType().Name} ({unityObject.name})"
                    : unityObject.name;
            }
            else
            {
                DisplayName = profile.DeclaringType.IsInterface
                    ? $"({target.GetType().Name})"
                    : profile.DeclaringType.Name;
            }

            UniqueID = backingID++;
            Enabled = profile.DefaultEnabled;
            Name = profile.MemberInfo.Name;
        }

        #endregion


        //--------------------------------------------------------------------------------------------------------------


        #region Overrides & Interfaces

        public virtual void Dispose()
        {
            if (Profile.ReceiveTick)
            {
                Monitor.MonitoringUpdateEvents.RemoveUpdateTicker(this);
            }
            RaiseDisposing();

            Disposing = null;
            ValueUpdated = null;
        }

        public override string ToString()
        {
            return $"[{Profile.DeclaringType}.{Name}]";
        }

        #endregion


        #region Obsolete

        [Obsolete] public string TargetName => DisplayName;

        #endregion
    }
}